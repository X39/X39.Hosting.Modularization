using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Configuration;
using X39.Hosting.Modularization.Exceptions;
using X39.Util.Collections;
using X39.Util.Threading;

namespace X39.Hosting.Modularization;

/// <summary>
/// Represents a module.
/// </summary>
[PublicAPI]
public sealed class ModuleContext : IAsyncDisposable
{
    /// <summary>
    /// The configuration of this module.
    /// </summary>
    public ModuleConfiguration Configuration { get; private set; }

    /// <summary>
    /// The last time the file, backing the <see cref="Configuration"/> was written to.
    /// </summary>
    /// <remarks>
    /// Is being used by <see cref="ModuleLoader"/> to determine when to trigger configuration reloads
    /// using <see cref="ChangeModuleConfigurationAsync"/>
    /// </remarks>
    internal DateTime ConfigurationLastWrittenToTimeStamp { get; private set; }

    /// <summary>
    /// Convenience property for getting the <see cref="Guid"/> from the <see cref="Configuration"/>.
    /// Returns the value of <see cref="ModuleConfiguration.Guid"/>.
    /// </summary>
    public Guid Guid => Configuration.Guid;

    /// <summary>
    /// Returns the path to the module's main assembly (dll).
    /// </summary>
    public string AssemblyPath => Path.Combine(ModuleDirectory, Configuration.StartDll);

    /// <summary>
    /// Returns the path to the module's main assemblies directory.
    /// </summary>
    public string ModuleDirectory { get; }

    /// <summary>
    /// The <see cref="ModuleContext"/>'s this module depends on.
    /// </summary>
    /// <remarks>
    /// All modules this module depends on must be loaded before this module can be loaded.
    /// </remarks>
    public IReadOnlyCollection<ModuleContext> Dependencies => _dependencies.AsReadOnly();

    /// <summary>
    /// The <see cref="ModuleContext"/>'s which depend on this module.
    /// </summary>
    /// <remarks>
    /// All modules listed must be unloaded before this module can be unloaded.
    /// </remarks>
    public IReadOnlyCollection<ModuleContext> Dependants => _dependants.AsReadOnly();

    /// <summary>
    /// Always <see langword="true"/> unless one or more dependencies for this module have not been loaded in (yet).
    /// </summary>
    /// <remarks>
    /// This property is altered by the <see cref="ModuleLoader"/> this <see cref="ModuleContext"/> was created from.
    /// </remarks>
    public bool AreDependenciesResolved { get; internal set; }

    /// <summary>
    /// <see cref="Boolean"/> indicating whether this <see cref="ModuleContext"/>
    /// can be loaded using <see cref="LoadAsync"/>
    /// </summary>
    public bool CanLoad => Dependencies.All((q) => q.IsLoaded)
                           && AreDependenciesResolved
                           && !IsLoaded
                           && !IsLoadingStateChanging;

    /// <summary>
    /// <see cref="Boolean"/> indicating whether this <see cref="ModuleContext"/>
    /// can be unloaded using <see cref="UnloadAsync"/>
    /// </summary>
    public bool CanUnload => Dependants.None((q) => q.IsLoaded)
                             && IsLoaded
                             && !IsLoadingStateChanging;

    /// <summary>
    /// Whether this module is loaded.
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    /// If set to true, this module is currently either loading or unloading.
    /// </summary>
    public bool IsLoadingStateChanging { get; private set; }

    private readonly List<ModuleContext> _dependants   = new();
    private readonly List<ModuleContext> _dependencies = new();
    private          ModuleLoadContext?  _assemblyLoadContext;
    private readonly IServiceProvider    _serviceProvider;
    private readonly ModuleLoader        _moduleLoader;
    private readonly SemaphoreSlim       _semaphoreSlim = new(1, 1);

    /// <summary>
    /// The actual instance of the main <see langword="class"/> of the module represented by this
    /// <see cref="ModuleContext"/>.
    /// </summary>
    public IModuleMain? Instance { get; private set; }

    /// <summary>
    /// The <see cref="System.Reflection.Assembly"/> loaded by the <see cref="ModuleContext"/>.
    /// </summary>
    public Assembly? Assembly { get; private set; }

    internal ModuleContext(
        ModuleLoader moduleLoader,
        IServiceProvider serviceProvider,
        string moduleDirectory,
        ModuleConfiguration configuration,
        DateTime configLastWriteTime)
    {
        _moduleLoader                       = moduleLoader;
        ModuleDirectory                     = moduleDirectory;
        Configuration                       = configuration;
        ConfigurationLastWrittenToTimeStamp = configLastWriteTime;
        _serviceProvider                    = serviceProvider;
    }

    internal void ChangeModuleConfigurationAsync(ModuleConfiguration configuration, DateTime timeStampLastWrittenTo)
    {
        if (configuration.Guid != Configuration.Guid)
            throw new ModuleConfigurationGuidCannotBeChangedException(this);
        if (IsLoaded && configuration.Build.Version != Configuration.Build.Version)
            throw new ModuleConfigurationBuildVersionCannotBeChangedException(this);
        if (configuration.StartDll != Configuration.StartDll)
            throw new ModuleConfigurationStartDllCannotBeChangedException(this);
        if (Configuration.Dependencies
            .Select((q) => (q.Guid, q.Version))
            .OrderBy((q) => q.Guid)
            .SequenceEqual(
                configuration.Dependencies
                    .Select((q) => (q.Guid, q.Version))
                    .OrderBy((q) => q.Guid)))
            throw new ModuleConfigurationDependenciesChangedForLoadedModuleException(this);
        ConfigurationLastWrittenToTimeStamp = timeStampLastWrittenTo;
        Configuration                       = configuration;
    }

    private void CreateAssemblyLoadContext()
    {
        var assemblyLoadContextName = string.Concat(
            Path.GetFileNameWithoutExtension(Configuration.StartDll),
            "-",
            Configuration.Guid.ToString());

        var loggerFactory = _serviceProvider.GetService<ILoggerFactory>()
                            ?? throw new NullReferenceException(
                                $"Failed to get logger factory ({typeof(ILoggerFactory).FullName()}) " +
                                $"from service provider ({_serviceProvider.GetType().FullName()}.");
        _assemblyLoadContext = new ModuleLoadContext(
            loggerFactory.CreateLogger<ModuleLoadContext>(),
            this,
            assemblyLoadContextName);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (IsLoaded)
            throw new ModuleStillLoadedException(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Loads this module into the application.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> to cancel the load operation, leaving the module unloaded.
    /// </param>
    /// <exception cref="ModuleDependencyNotLoadedException">
    ///     Thrown when a module has dependencies set in the <see cref="ModuleConfiguration"/> but they are not loaded
    ///     (yet).
    ///     Please note that this differs from the <see cref="CannotResolveModuleDependenciesException"/> exception by
    ///     being a pre-loading exception (i.e. the module is not loaded yet) and talks about other
    ///     <see cref="ModuleContext"/>'s not being loaded, not types not being available in dependency injection.
    /// </exception>
    /// <exception cref="ModuleAlreadyLoadedException">
    ///     Thrown when this module is already loaded.
    /// </exception>
    /// <exception cref="ModuleAlreadyLoadingException">
    ///     Thrown when this module is already loading.
    /// </exception>
    /// <exception cref="NoModuleMainTypeException">
    ///     Thrown when the module's main assembly does not contain a type implementing <see cref="IModuleMain"/>.
    /// </exception>
    /// <exception cref="ModuleMainTypeIsGenericException">
    ///     Thrown when the module's main assembly contains a type implementing <see cref="IModuleMain"/> but
    ///     the type is generic.
    /// </exception>
    /// <exception cref="MultipleModuleMainTypesException">
    ///     Thrown when the module's main assembly contains multiple types implementing <see cref="IModuleMain"/>.
    /// </exception>
    /// <exception cref="MultipleMainTypeConstructorsException">
    ///     Thrown when the module's main assembly contains a type implementing <see cref="IModuleMain"/> but
    ///     the type has multiple constructors.
    /// </exception>
    /// <exception cref="CannotResolveModuleDependenciesException">
    ///     Thrown when the module's dependencies could not be resolved by the <see cref="IServiceProvider"/>.
    ///     Please note that this differs from the <see cref="ModuleDependencyNotLoadedException"/> exception by
    ///     being a post-loading exception (i.e. the module is already loaded but not initialized)
    ///     and is specific to dependency injection.
    /// </exception>
    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        await _moduleLoader.ModuleLoadSemaphore.LockedAsync(
            async () =>
            {
                if (Dependencies.Any(moduleContext => !moduleContext.IsLoaded))
                    throw new ModuleDependencyNotLoadedException(
                        this,
                        Dependencies.Where((moduleContext) => !moduleContext.IsLoaded).ToImmutableArray());
                await LoadModuleAsync(cancellationToken)
                    .ConfigureAwait(false);
            },
            cancellationToken);
    }

    /// <summary>
    /// Unloads this module from the application.
    /// </summary>
    /// <exception cref="ModuleDependantsNotUnloadedException">
    ///     Thrown when this module has dependants which are still loaded.
    /// </exception>
    public async Task UnloadAsync()
    {
        await _moduleLoader.ModuleLoadSemaphore.LockedAsync(
            async () =>
            {
                if (Dependants.Any(moduleContext => moduleContext.IsLoaded))
                    throw new ModuleDependantsNotUnloadedException(
                        this,
                        Dependencies.Where((moduleContext) => moduleContext.IsLoaded).ToImmutableArray());
                await UnloadModuleAsync()
                    .ConfigureAwait(false);
            });
    }

    internal async Task LoadModuleAsync(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.LockedAsync(
                async () =>
                {
                    if (IsLoaded)
                        throw new ModuleAlreadyLoadedException(this);
                    if (IsLoadingStateChanging)
                        throw new ModuleAlreadyLoadingException(this);
                    IsLoadingStateChanging = true;

                    await Fault.IgnoreAsync(async () => await _moduleLoader.OnModuleLoading(this));

                    using var loadedUnset = new Disposable(() => IsLoadingStateChanging = false);
                    Debug.Assert(_assemblyLoadContext is null, "_assemblyLoadContext is not null");
                    CreateAssemblyLoadContext();
                    if (_assemblyLoadContext is null)
                        throw new NullReferenceException("_assemblyLoadContext is null");
                    Assembly = _assemblyLoadContext.LoadFromAssemblyPath(AssemblyPath);

                    await Fault.IgnoreAsync(async () => await _moduleLoader.OnModuleAssemblyLoaded(this, Assembly));
                    var mainType = GetMainType(Assembly);
                    var constructor = GetMainConstructorOrNull(mainType);
                    Instance = default(IModuleMain);
                    try
                    {
                        Instance = ResolveType(constructor, mainType);
                        await Instance.LoadModuleAsync(cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch
                    {
                        if (Instance is not null)
                            await Fault.IgnoreAsync(async () => await Instance.DisposeAsync())
                                .ConfigureAwait(false);
                        _assemblyLoadContext.Unload();
                        throw;
                    }

                    IsLoaded = true;
                },
                cancellationToken)
            .ConfigureAwait(false);
        await Fault.IgnoreAsync(async () => await _moduleLoader.OnModuleLoaded(this))
            .ConfigureAwait(false);
    }

    private IModuleMain ResolveType(ConstructorInfo? constructor, Type mainType)
    {
        IModuleMain instance;
        if (constructor is null)
        {
            instance = (IModuleMain) mainType.CreateInstance(Type.EmptyTypes, Array.Empty<object>());
        }
        else
        {
            var parameters = constructor.GetParameters();
            var services = parameters.Select(
                    (parameterInfo) => (parameterInfo,
                        value: parameterInfo.ParameterType.IsEquivalentTo(typeof(ModuleContext))
                            ? this
                            : _serviceProvider.GetService(parameterInfo.ParameterType)))
                .ToImmutableArray();
            var nullViolatingServices = services
                .Indexed()
                .Where((tuple) => tuple.value.value is null)
                .Where((tuple) => tuple.value.parameterInfo.IsNullable())
                .ToImmutableArray();
            if (nullViolatingServices.Any())
            {
                throw new CannotResolveModuleDependenciesException(
                    this,
                    nullViolatingServices
                        .Select((tuple) => (tuple.index, tuple.value.parameterInfo.ParameterType))
                        .ToImmutableArray());
            }

            instance = (IModuleMain) mainType.CreateInstanceWithUncached(
                services.Select((tuple) => tuple.parameterInfo.ParameterType).ToArray(),
                services.Select((tuple) => tuple.value).ToArray());
        }

        return instance;
    }

    private ConstructorInfo? GetMainConstructorOrNull(Type mainType)
    {
        var candidates = mainType.GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        switch (candidates.Length)
        {
            case > 1:
                var typeName = mainType.FullName();
                _assemblyLoadContext?.Unload();
                _assemblyLoadContext = null;
                throw new MultipleMainTypeConstructorsException(this, typeName);
        }

        var constructor = candidates.SingleOrDefault();
        return constructor;
    }

    private Type GetMainType(Assembly assembly)
    {
        var assemblyTypes = assembly.GetTypes();
        var query = from type in assemblyTypes
            where type.IsAssignableTo(typeof(IModuleMain))
            where !type.IsAbstract
            select type;
        var candidates = query.ToImmutableArray();
        switch (candidates.Length)
        {
            default:
                _assemblyLoadContext?.Unload();
                _assemblyLoadContext = null;
                throw new NoModuleMainTypeException(this);
            case 1:
                var mainType = candidates.Single();
                if (!mainType.IsGenericType)
                    return mainType;
                var typeName = mainType.FullName();
                _assemblyLoadContext?.Unload();
                _assemblyLoadContext = null;
                throw new ModuleMainTypeIsGenericException(this, typeName);
            case > 1:
                var typeNames = candidates.Select((q) => q.FullName());
                _assemblyLoadContext?.Unload();
                _assemblyLoadContext = null;
                throw new MultipleModuleMainTypesException(this, typeNames);
        }
    }

    internal async Task UnloadModuleAsync()
    {
        await _semaphoreSlim.LockedAsync(
                async () =>
                {
                    if (!IsLoaded)
                        throw new ModuleIsNotLoadedException(this);
                    if (IsLoadingStateChanging)
                        throw new ModuleLoadingNotFinishedException(this);
                    if (_assemblyLoadContext is null)
                        throw new NullReferenceException("_assemblyLoadContext is null");
                    IsLoadingStateChanging = true;

                    await Fault.IgnoreAsync(async () => await _moduleLoader.OnModuleUnloading(this));
                    using var loadedUnset = new Disposable(() => IsLoadingStateChanging = false);

                    // We do not try-catch around this as exceptions here actually hinder unloading
                    if (Instance is not null)
                        await Instance.DisposeAsync();
                    Instance = null;
                    Assembly = null;
                    GC.Collect();
                    GC.WaitForFullGCComplete();
                    GC.WaitForPendingFinalizers();
                    _assemblyLoadContext!.Unload();
                    _assemblyLoadContext = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    IsLoaded = false;
                })
            .ConfigureAwait(false);
        await Fault.IgnoreAsync(async () => await _moduleLoader.OnModuleUnloaded(this))
            .ConfigureAwait(false);
    }

    internal void AddDependency(ModuleContext dependency)
    {
        _dependencies.Add(dependency);
    }

    internal void AddDependencies(IEnumerable<ModuleContext> dependencies)
    {
        _dependencies.AddRange(dependencies);
    }

    internal void AddDependant(ModuleContext moduleContext)
    {
        _dependants.Add(moduleContext);
    }
}