using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Configuration;
using X39.Hosting.Modularization.Exceptions;
using X39.Util.Collections;
using X39.Util.Threading;

namespace X39.Hosting.Modularization;

[PublicAPI]
[SuppressMessage("Naming", "CA1720")]
public abstract class ModuleContextBase : IAsyncDisposable
{
    private readonly List<ModuleContextBase> _dependants    = new();
    private readonly List<ModuleContextBase> _dependencies  = new();
    private readonly SemaphoreSlim           _semaphoreSlim = new(1, 1);

    /// <summary>
    /// Creates a new <see cref="ModuleContextBase"/>.
    /// </summary>
    /// <param name="moduleLoader"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="configuration"></param>
    protected ModuleContextBase(
        ModuleLoader moduleLoader,
        IServiceProvider serviceProvider,
        ModuleConfiguration configuration)
    {
        Configuration         = configuration;
        ModuleLoader          = moduleLoader;
        MasterServiceProvider = serviceProvider;
    }

    /// <summary>
    /// The master service provider.
    /// </summary>
    protected IServiceProvider MasterServiceProvider { get; private set; }

    /// <summary>
    /// The module loader of this module.
    /// </summary>
    protected ModuleLoader ModuleLoader { get; private set; }

    /// <summary>
    /// The service collection of the module.
    /// </summary>
    protected ServiceCollection? ServiceCollection { get; set; }

    /// <summary>
    /// The configuration of this module.
    /// </summary>
    public ModuleConfiguration Configuration { get; protected internal set; }

    /// <summary>
    /// Convenience property for getting the <see cref="Guid"/> from the <see cref="Configuration"/>.
    /// Returns the value of <see cref="ModuleConfiguration.Guid"/>.
    /// </summary>
    public Guid Guid => Configuration.Guid;

    /// <summary>
    /// The <see cref="ModuleContextBase"/>'s this module depends on.
    /// </summary>
    /// <remarks>
    /// All modules this module depends on must be loaded before this module can be loaded.
    /// </remarks>
    public IReadOnlyCollection<ModuleContextBase> Dependencies => _dependencies.AsReadOnly();

    /// <summary>
    /// The <see cref="ModuleContextBase"/>'s which depend on this module.
    /// </summary>
    /// <remarks>
    /// All modules listed must be unloaded before this module can be unloaded.
    /// </remarks>
    public IReadOnlyCollection<ModuleContextBase> Dependants => _dependants.AsReadOnly();

    /// <summary>
    /// Always <see langword="true"/> unless one or more dependencies for this module have not been loaded in (yet).
    /// </summary>
    /// <remarks>
    /// This property is altered by the <see cref="Modularization.ModuleLoader"/> this <see cref="ModuleContextBase"/> was created from.
    /// </remarks>
    public bool AreDependenciesResolved { get; internal set; }

    /// <summary>
    /// <see cref="Boolean"/> indicating whether this <see cref="ModuleContextBase"/>
    /// can be loaded using <see cref="LoadAsync"/>
    /// </summary>
    public bool CanLoad => Dependencies.All((q) => q.IsLoaded)
                           && AreDependenciesResolved
                           && !IsLoaded
                           && !IsLoadingStateChanging;

    /// <summary>
    /// <see cref="Boolean"/> indicating whether this <see cref="ModuleContextBase"/>
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

    /// <summary>
    /// The actual instance of the main <see langword="class"/> of the module represented by this
    /// <see cref="ModuleContextBase"/>.
    /// </summary>
    public IModuleMain? Instance { get; protected set; }

    /// <summary>
    /// The <see cref="IServiceProvider"/> specific to this <see cref="ModuleContextBase"/>.
    /// </summary>
    public IServiceProvider? ServiceProvider { get; protected internal set; }

    /// <inheritdoc />
#pragma warning disable CA1816
    public virtual ValueTask DisposeAsync()
#pragma warning restore CA1816
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
    ///     <see cref="ModuleContextBase"/>'s not being loaded, not types not being available in dependency injection.
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
        await ModuleLoader.ModuleLoadSemaphore.LockedAsync(
            async () =>
            {
                if (Dependencies.Any(moduleContextBase => !moduleContextBase.IsLoaded))
                    throw new ModuleDependencyNotLoadedException(
                        this,
                        Dependencies.Where((moduleContextBase) => !moduleContextBase.IsLoaded).ToImmutableArray());
                await _semaphoreSlim.LockedAsync(
                        async () =>
                        {
                            if (IsLoaded)
                                throw new ModuleAlreadyLoadedException(this);
                            if (IsLoadingStateChanging)
                                throw new ModuleAlreadyLoadingException(this);
                            await Fault.IgnoreAsync(async () => await ModuleLoader.OnModuleLoading(this));
                            IsLoadingStateChanging = true;
                            using var loadedUnset = new Disposable(() => IsLoadingStateChanging = false);
                            await DoLoadAsync(cancellationToken)
                                .ConfigureAwait(false);

                            IsLoaded = true;
                        },
                        cancellationToken)
                    .ConfigureAwait(false);
                await Fault.IgnoreAsync(async () => await ModuleLoader.OnModuleLoaded(this))
                    .ConfigureAwait(false);
            },
            cancellationToken);
    }

    /// <summary>
    /// Performs the loading of the module.
    /// </summary>
    protected internal abstract Task DoLoadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Unloads this module from the application.
    /// </summary>
    /// <exception cref="ModuleDependantsNotUnloadedException">
    ///     Thrown when this module has dependants which are still loaded.
    /// </exception>
    public async Task UnloadAsync()
    {
        await ModuleLoader.ModuleLoadSemaphore.LockedAsync(
            async () =>
            {
                if (Dependants.Any(moduleContextBase => moduleContextBase.IsLoaded))
                    throw new ModuleDependantsNotUnloadedException(
                        this,
                        Dependencies.Where((moduleContextBase) => moduleContextBase.IsLoaded).ToImmutableArray());
                await _semaphoreSlim.LockedAsync(
                        async () =>
                        {
                            if (!IsLoaded)
                                throw new ModuleIsNotLoadedException(this);
                            if (IsLoadingStateChanging)
                                throw new ModuleLoadingNotFinishedException(this);
                            IsLoadingStateChanging = true;

                            await Fault.IgnoreAsync(async () => await ModuleLoader.OnModuleUnloading(this));
                            using var loadedUnset = new Disposable(() => IsLoadingStateChanging = false);

                            await DoUnloadAsync()
                                .ConfigureAwait(false);

                            IsLoaded = false;
                        })
                    .ConfigureAwait(false);
                await Fault.IgnoreAsync(async () => await ModuleLoader.OnModuleUnloaded(this))
                    .ConfigureAwait(false);
            });
    }

    /// <summary>
    /// Performs the unloading of the module.
    /// </summary>
    protected internal abstract Task DoUnloadAsync();

    /// <summary>
    /// Disposes the <see cref="Instance"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
    protected async Task DisposeOfInstance()
    {
        switch (Instance)
        {
            case IAsyncDisposable asyncDisposable:
                await Fault.IgnoreAsync(async () => await asyncDisposable.DisposeAsync())
                    .ConfigureAwait(false);
                break;
            case IDisposable disposable:
                Fault.Ignore(() => disposable.Dispose());
                break;
        }

        Instance = null;
    }

    /// <summary>
    /// Disposes the <see cref="ServiceCollection"/>
    /// </summary>
    protected async Task DisposeOfServiceCollection()
    {
        if (ServiceCollection is not null)
        {
            foreach (var instance in ServiceCollection
                         .Select((q) => q.ImplementationInstance)
                         .NotNull())
            {
                switch (instance)
                {
                    case IAsyncDisposable asyncDisposable:
                        await Fault.IgnoreAsync(async () => await asyncDisposable.DisposeAsync())
                            .ConfigureAwait(false);
                        break;
                    case IDisposable disposable:
                        Fault.Ignore(() => disposable.Dispose());
                        break;
                }
            }

            ServiceCollection = null;
        }
    }

    /// <summary>
    /// Creates the module instance.
    /// </summary>
    /// <param name="constructor">The constructor of <paramref name="mainType"/> to use.</param>
    /// <param name="mainType">The <see cref="Type"/> to construct.</param>
    /// <param name="provider">The <see cref="IServiceProvider"/> to use.</param>
    /// <returns></returns>
    /// <exception cref="CannotResolveModuleDependenciesException">
    ///     Thrown when one or more module dependencies could not be resolved.
    /// </exception>
    protected IModuleMain ResolveType(ConstructorInfo? constructor, Type mainType, IServiceProvider provider)
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
                        value: parameterInfo.ParameterType.IsEquivalentTo(typeof(ModuleContextBase))
                            ? this
                            : provider.GetService(parameterInfo.ParameterType)))
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

    /// <summary>
    /// Creates a new <see cref="HierarchicalServiceProvider"/> containing the dependencies.
    /// </summary>
    /// <returns>The new <see cref="HierarchicalServiceProvider"/>.</returns>
    protected HierarchicalServiceProvider CreateHierarchicalServiceProvider()
    {
        var providers = Dependencies.Select((q) => q.ServiceProvider).NotNull();
        var list = new List<IServiceProvider>();
        foreach (var serviceProvider in providers)
        {
            if (serviceProvider is not HierarchicalServiceProvider sub)
                list.Add(serviceProvider);
            else
                list.AddRange(sub.GetServiceProviders());
        }

        var serviceProviders = list.Prepend(MasterServiceProvider);
        if (ServiceCollection is not null)
            serviceProviders = serviceProviders.Prepend(ServiceCollection.BuildServiceProvider());
        var hierarchicalServiceProvider = new HierarchicalServiceProvider(serviceProviders.Distinct());
        hierarchicalServiceProvider.CreateAsyncScope();
        return hierarchicalServiceProvider;
    }

    /// <summary>
    /// Returns the constructor of <paramref name="mainType"/>.
    /// </summary>
    /// <param name="mainType">The type to search the constructor of.</param>
    /// <returns>The constructor info if available or null if no explicit constructor exists.</returns>
    /// <exception cref="MultipleMainTypeConstructorsException">
    ///     Thrown when more then one constructor was found.
    /// </exception>
    protected ConstructorInfo? GetMainConstructorOrNull(Type mainType)
    {
        var candidates = mainType.GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        switch (candidates.Length)
        {
            case > 1:
                var typeName = mainType.FullName();
                throw new MultipleMainTypeConstructorsException(this, typeName);
        }

        var constructor = candidates.SingleOrDefault();
        return constructor;
    }

    internal void AddDependency(ModuleContextBase dependency)
    {
        _dependencies.Add(dependency);
    }

    internal void AddDependencies(IEnumerable<ModuleContextBase> dependencies)
    {
        _dependencies.AddRange(dependencies);
    }

    internal void AddDependant(ModuleContextBase moduleContextBase)
    {
        _dependants.Add(moduleContextBase);
    }
}