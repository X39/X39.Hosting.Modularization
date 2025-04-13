using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Configuration;
using X39.Hosting.Modularization.DependencyInjection;
using X39.Hosting.Modularization.Exceptions;

namespace X39.Hosting.Modularization;

/// <summary>
/// Represents a module.
/// </summary>
[PublicAPI]
[SuppressMessage("Usage", "CA2201")]
public sealed class ModuleContext : ModuleContextBase
{
    /// <summary>
    /// Returns the path to the module's main assembly (dll).
    /// </summary>
    public string AssemblyPath => Path.Combine(ModuleDirectory, Configuration.StartDll);

    /// <summary>
    /// Returns the path to the module's main assemblies directory.
    /// </summary>
    public string ModuleDirectory { get; }

    /// <summary>
    /// The <see cref="System.Reflection.Assembly"/> loaded by the <see cref="ModuleContext"/>.
    /// </summary>
    public Assembly? Assembly { get; private set; }

    /// <summary>
    /// The last time the file, backing the <see cref="Configuration"/> was written to.
    /// </summary>
    /// <remarks>
    /// Is being used by <see cref="ModuleLoader"/> to determine when to trigger configuration reloads
    /// using <see cref="ModuleContext.ChangeModuleConfigurationAsync"/>
    /// </remarks>
    internal DateTime ConfigurationLastWrittenToTimeStamp { get; private set; }

    /// <inheritdoc />
    public override bool CanUnload => base.CanUnload && !Configuration.DisableUnload;

    private ModuleLoadContext? _assemblyLoadContext;

    internal ModuleContext(
        ModuleLoader moduleLoader,
        IServiceProvider serviceProvider,
        string moduleDirectory,
        ModuleConfiguration configuration,
        DateTime configLastWriteTime
    )
        : base(moduleLoader, serviceProvider, configuration)
    {
        ModuleDirectory                     = moduleDirectory;
        ConfigurationLastWrittenToTimeStamp = configLastWriteTime;
    }

    internal void ChangeModuleConfigurationAsync(ModuleConfiguration configuration, DateTime timeStampLastWrittenTo)
    {
        if (configuration.Guid != Configuration.Guid)
            throw new ModuleConfigurationGuidCannotBeChangedException(this);
        if (IsLoaded && configuration.Build.Version != Configuration.Build.Version)
            throw new ModuleConfigurationBuildVersionCannotBeChangedException(this);
        if (configuration.StartDll != Configuration.StartDll)
            throw new ModuleConfigurationStartDllCannotBeChangedException(this);
        if (configuration.DisableUnload != Configuration.DisableUnload)
            throw new ModuleConfigurationDisableUnloadCannotBeChangedException(this);
        if (Configuration.Dependencies
            .Select((q) => (q.Guid, q.Version))
            .OrderBy((q) => q.Guid)
            .SequenceEqual(
                configuration.Dependencies
                    .Select((q) => (q.Guid, q.Version))
                    .OrderBy((q) => q.Guid)
            ))
            throw new ModuleConfigurationDependenciesChangedForLoadedModuleException(this);
        ConfigurationLastWrittenToTimeStamp = timeStampLastWrittenTo;
        Configuration                       = configuration;
    }

    private void CreateAssemblyLoadContext()
    {
        var assemblyLoadContextName = string.Concat(
            Path.GetFileNameWithoutExtension(Configuration.StartDll),
            "-",
            Configuration.Guid.ToString()
        );

        var loggerFactory = MasterServiceProvider.GetService<ILoggerFactory>()
                            ?? throw new NullReferenceException(
                                $"Failed to get logger factory ({typeof(ILoggerFactory).FullName()}) "
                                + $"from service provider ({MasterServiceProvider.GetType().FullName()}."
                            );
        _assemblyLoadContext = new ModuleLoadContext(
            loggerFactory.CreateLogger<ModuleLoadContext>(),
            this,
            assemblyLoadContextName,
            () => GetDependencyLoadContexts()
                .Prepend(AssemblyLoadContext.Default)
        );
    }

    private IEnumerable<AssemblyLoadContext> GetDependencyLoadContexts()
    {
        List<(int level, AssemblyLoadContext assemblyLoadContext)> contexts = new();

        void Recurse(ModuleContext moduleContext, int level = 0)
        {
            foreach (var dependency in moduleContext.Dependencies.OfType<ModuleContext>())
            {
                Recurse(dependency, level + 1);
            }

            var context = moduleContext._assemblyLoadContext;
            if (context is null)
                throw new NullReferenceException($"AssemblyLoadContext of ModuleContext {moduleContext.Guid} is null");
            contexts.Add((level, context));
        }

        foreach (var dependency in Dependencies.OfType<ModuleContext>())
        {
            Recurse(dependency);
        }

        return contexts.OrderByDescending((q) => q.level)
            .Select((q) => q.assemblyLoadContext);
    }


    /// <inheritdoc />
    protected internal override async Task DoLoadAsync(CancellationToken cancellationToken)
    {
        Debug.Assert(_assemblyLoadContext is null, "_assemblyLoadContext is not null");
        CreateAssemblyLoadContext();
        if (_assemblyLoadContext is null)
            throw new NullReferenceException("_assemblyLoadContext is null");
        Assembly = _assemblyLoadContext.LoadFromAssemblyPath(AssemblyPath);

        await Fault.IgnoreAsync(async () => await ModuleLoader.OnModuleAssemblyLoaded(this, Assembly));
        var mainType = GetMainType(Assembly);
        try
        {
            var constructor = GetMainConstructorOrNull(mainType);
            Instance          = default(IModuleMain);
            ServiceCollection = [];
            var hierarchicalServiceProvider = CreateHierarchicalServiceProvider();
            Instance = ResolveType(constructor, mainType, hierarchicalServiceProvider);
            await Instance.ConfigureServicesAsync(ServiceCollection, cancellationToken);
            var provider = ServiceCollection.BuildServiceProvider();
            hierarchicalServiceProvider.Set(provider);
            ServiceProvider = hierarchicalServiceProvider;
            await Instance.ConfigureAsync(ServiceProvider, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            await DisposeOfInstance();
            await DisposeOfServiceCollection();
            Assembly = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            _assemblyLoadContext.Unload();
            _assemblyLoadContext = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            throw;
        }
    }

    private Type GetMainType(Assembly assembly)
    {
        var assemblyTypes = assembly.GetTypes();
        var query =
            from type in assemblyTypes
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

    /// <inheritdoc />
    protected internal override async Task DoUnloadAsync()
    {
        if (_assemblyLoadContext is null)
            throw new NullReferenceException("_assemblyLoadContext is null");
        // We do not try-catch around this as exceptions here actually hinder unloading
        await DisposeOfInstance();
        await DisposeOfServiceCollection();
        Assembly = null;
        GC.Collect();
        GC.WaitForFullGCComplete();
        GC.WaitForPendingFinalizers();
        _assemblyLoadContext!.Unload();
        _assemblyLoadContext = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}
