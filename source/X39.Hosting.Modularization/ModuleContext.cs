using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Abstraction.Attributes;
using X39.Hosting.Modularization.Configuration;
using X39.Hosting.Modularization.Exceptions;

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
    /// Returns the path to the module's main assembly.
    /// </summary>
    public string AssemblyPath => Path.Combine(_moduleDirectory, Configuration.StartDll);

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
    /// If set to true, this module is currently loading.
    /// </summary>
    public bool IsLoading { get; private set; }

    private readonly List<ModuleContext> _dependants   = new();
    private readonly List<ModuleContext> _dependencies = new();
    private readonly AssemblyLoadContext _assemblyLoadContext;
    private readonly string              _moduleDirectory;
    private readonly IServiceProvider    _serviceProvider;

    internal ModuleContext(
        IServiceProvider serviceProvider,
        string moduleDirectory,
        ModuleConfiguration configuration)
    {
        _moduleDirectory = moduleDirectory;
        Configuration    = configuration;
        var assemblyLoadContextName = string.Concat(
            Path.GetFileNameWithoutExtension(configuration.StartDll),
            "-",
            configuration.Guid.ToString());

        _serviceProvider = serviceProvider;
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>()
                            ?? throw new NullReferenceException(
                                $"Failed to get logger factory ({typeof(ILoggerFactory).FullName()}) " +
                                $"from service provider ({serviceProvider.GetType().FullName()}.");
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

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (Dependencies.Any(moduleContext => !moduleContext.IsLoaded))
            throw new ModuleDependencyNotLoadedException(
                this,
                Dependencies.Where((moduleContext) => !moduleContext.IsLoaded).ToImmutableArray());
        await LoadModuleAsync(cancellationToken);
    }

    public async Task UnloadAsync(CancellationToken cancellationToken)
    {
        if (Dependants.Any(moduleContext => moduleContext.IsLoaded))
            throw new ModuleDependantsNotUnloadedException(
                this,
                Dependencies.Where((moduleContext) => moduleContext.IsLoaded).ToImmutableArray());
        await UnloadModuleAsync(cancellationToken);
    }

    internal async Task LoadModuleAsync(CancellationToken cancellationToken)
    {
        lock (this)
        {
            if (IsLoaded)
                throw new ModuleAlreadyLoadedException(this);
            if (IsLoading)
                throw new ModuleAlreadyLoadingException(this);
            IsLoading = true;
        }

        using var loadedUnset = new Disposable(() => IsLoading = false);

        // ToDo: use Disposable to ensure isloaded is set to false
        var assembly = _assemblyLoadContext.LoadFromAssemblyPath(AssemblyPath);
        var mainType = GetMainType(assembly);
        var constructor = GetMainConstructorOrNull(mainType);

        object instance;
        if (constructor is null)
        {
            // ToDo: Catch exception if thrown and auto-unload
            instance = mainType.CreateInstance();
        }
        else
        {
            var parameters = constructor.GetParameters();
            var types = parameters.Select((parameterInfo) => parameterInfo.ParameterType);
            var services = types.Select(
                (type) => type.IsEquivalentTo(typeof(ModuleContext))
                    ? this
                    : _serviceProvider.GetService(type));
            // ToDo: Check nullability and throw an exception if not matched.
        }


        IsLoaded = true;
    }

    private ConstructorInfo? GetMainConstructorOrNull(Type mainType)
    {
        var candidates = mainType.GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        switch (candidates.Length)
        {
            case > 1:
                var typeName = mainType.FullName();
                _assemblyLoadContext.Unload();
                throw new MultipleMainTypeConstructorsException(this, typeName);
        }

        var constructor = candidates.SingleOrDefault();
        return constructor;
    }

    private Type GetMainType(Assembly assembly)
    {
        var query = from type in assembly.GetTypes()
            select (type, attribute: type.GetCustomAttribute<ModuleMainAttribute>())
            into tuple
            where tuple.attribute is not null
            select tuple.type;
        var candidates = query.ToImmutableArray();
        switch (candidates.Length)
        {
            case 0:
                _assemblyLoadContext.Unload();
                throw new NoModuleMainTypeException(this);
            case > 1:
                var typeNames = candidates.Select((q) => q.FullName());
                _assemblyLoadContext.Unload();
                throw new MultipleModuleMainTypesException(this, typeNames);
        }

        var mainType = candidates.Single();
        if (mainType.IsGenericType)
        {
            var typeName = mainType.FullName();
            _assemblyLoadContext.Unload();
            throw new ModuleMainTypeIsGenericException(this, typeName);
        }

        return mainType;
    }

    internal async Task UnloadModuleAsync(CancellationToken cancellationToken)
    {
        lock (this)
        {
            if (!IsLoaded)
                throw new ModuleIsNotLoadedException(this);
            if (IsLoading)
                throw new ModuleLoadingNotFinishedException(this);
            IsLoading = true;
        }

        throw new NotImplementedException();
        _assemblyLoadContext.Unload();
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