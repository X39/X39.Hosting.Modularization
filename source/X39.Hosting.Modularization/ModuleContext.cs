using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Configuration;
using X39.Hosting.Modularization.Exceptions;
using X39.Util.Collections;

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
    public ModuleConfiguration Configuration { get; }

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

    /// <summary>
    /// The actual instance of the main <see langword="class"/> of the module represented by this
    /// <see cref="ModuleContext"/>.
    /// </summary>
    public IModuleMain? Instance { get; private set; }

    internal ModuleContext(
        IServiceProvider serviceProvider,
        string moduleDirectory,
        ModuleConfiguration configuration)
    {
        ModuleDirectory  = moduleDirectory;
        Configuration    = configuration;
        _serviceProvider = serviceProvider;
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
        if (Dependencies.Any(moduleContext => !moduleContext.IsLoaded))
            throw new ModuleDependencyNotLoadedException(
                this,
                Dependencies.Where((moduleContext) => !moduleContext.IsLoaded).ToImmutableArray());
        await LoadModuleAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Unloads this module from the application.
    /// </summary>
    /// <exception cref="ModuleDependantsNotUnloadedException">
    ///     Thrown when this module has dependants which are still loaded.
    /// </exception>
    public async Task UnloadAsync()
    {
        if (Dependants.Any(moduleContext => moduleContext.IsLoaded))
            throw new ModuleDependantsNotUnloadedException(
                this,
                Dependencies.Where((moduleContext) => moduleContext.IsLoaded).ToImmutableArray());
        await UnloadModuleAsync()
            .ConfigureAwait(false);
    }

    internal async Task LoadModuleAsync(CancellationToken cancellationToken)
    {
        lock (this)
        {
            if (IsLoaded)
                throw new ModuleAlreadyLoadedException(this);
            if (IsLoadingStateChanging)
                throw new ModuleAlreadyLoadingException(this);
            IsLoadingStateChanging = true;
        }

        using var loadedUnset = new Disposable(() => IsLoadingStateChanging = false);
        Debug.Assert(_assemblyLoadContext is null, "_assemblyLoadContext is not null");
        CreateAssemblyLoadContext();
        if (_assemblyLoadContext is null)
            throw new NullReferenceException("_assemblyLoadContext is null");
        var assembly = _assemblyLoadContext.LoadFromAssemblyPath(AssemblyPath);
        var mainType = GetMainType(assembly);
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
    }

    private IModuleMain ResolveType(ConstructorInfo? constructor, Type mainType)
    {
        IModuleMain instance;
        if (constructor is null)
        {
            instance = (IModuleMain) mainType.CreateInstanceWithUncached(Type.EmptyTypes, Array.Empty<object>());
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
        lock (this)
        {
            if (!IsLoaded)
                throw new ModuleIsNotLoadedException(this);
            if (IsLoadingStateChanging)
                throw new ModuleLoadingNotFinishedException(this);
            if (_assemblyLoadContext is null)
                throw new NullReferenceException("_assemblyLoadContext is null");
            IsLoadingStateChanging = true;
        }

        using var loadedUnset = new Disposable(() => IsLoadingStateChanging = false);

        try
        {
            if (Instance is not null)
                await Instance.DisposeAsync();
        }
        finally
        {
            Instance = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            _assemblyLoadContext.Unload();
            _assemblyLoadContext = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            IsLoaded = false;
        }
    }

    internal void AddDependencies(IEnumerable<ModuleContext> dependencies)
    {
        _dependencies.AddRange(dependencies);
    }

    internal void AddDependants(ModuleContext moduleContext)
    {
        _dependants.Add(moduleContext);
    }
}