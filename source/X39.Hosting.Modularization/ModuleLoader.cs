using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Configuration;
using X39.Hosting.Modularization.Data;
using X39.Hosting.Modularization.Exceptions;
using X39.Util.Collections;
using X39.Util.Threading;

namespace X39.Hosting.Modularization;

/// <summary>
/// The modularization host class.
/// It is the main entry point for the modularization system, offering support for loading and unloading independent
/// software modules during runtime.
/// </summary>
[PublicAPI]
[SuppressMessage("Performance", "CA1848:LoggerMessage-Delegaten verwenden")]
public sealed class ModuleLoader : IAsyncDisposable
{
    private readonly IServiceProvider      _serviceProvider;
    private readonly ILogger<ModuleLoader> _logger;
    private readonly List<ModuleContext>   _moduleContexts = new();

    internal readonly SemaphoreSlim                               ModuleLoadSemaphore = new(1, 1);
    private readonly  List<string>                                _moduleDirectories  = new();

    /// <summary>
    /// Raised when a <see cref="ModuleContext"/> created by this <see cref="ModuleLoader"/> is unloading.
    /// </summary>
    public event AsyncEventHandler<UnloadingEventArgs>? ModuleUnloading;

    /// <summary>
    /// Raised when a <see cref="ModuleContext"/> created by this <see cref="ModuleLoader"/> has finished unloading.
    /// </summary>
    public event AsyncEventHandler<UnloadedEventArgs>? ModuleUnloaded;

    /// <summary>
    /// Raised when a <see cref="ModuleContext"/> created by this <see cref="ModuleLoader"/> is being loaded.
    /// </summary>
    public event AsyncEventHandler<LoadingEventArgs>? ModuleLoading;

    /// <summary>
    /// Raised when a <see cref="ModuleContext"/> created by this <see cref="ModuleLoader"/>
    /// has loaded the <see cref="System.Reflection.Assembly"/>.
    /// </summary>
    public event AsyncEventHandler<AssemblyLoadedEventArgs>? ModuleAssemblyLoaded;

    /// <summary>
    /// Raised when a <see cref="ModuleContext"/> created by this <see cref="ModuleLoader"/> has finished loading.
    /// </summary>
    public event AsyncEventHandler<LoadedEventArgs>? ModuleLoaded;

    internal Task OnModuleUnloading(ModuleContext moduleContext)
        => ModuleUnloading.DynamicInvokeAsync(this, new UnloadingEventArgs(moduleContext));

    internal Task OnModuleUnloaded(ModuleContext moduleContext)
        => ModuleUnloaded.DynamicInvokeAsync(this, new UnloadedEventArgs(moduleContext));

    internal Task OnModuleLoading(ModuleContext moduleContext)
        => ModuleLoading.DynamicInvokeAsync(this, new LoadingEventArgs(moduleContext));

    internal Task OnModuleAssemblyLoaded(ModuleContext moduleContext, Assembly assembly)
        => ModuleAssemblyLoaded.DynamicInvokeAsync(this, new AssemblyLoadedEventArgs(moduleContext, assembly));

    internal Task OnModuleLoaded(ModuleContext moduleContext)
        => ModuleLoaded.DynamicInvokeAsync(this, new LoadedEventArgs(moduleContext));

    /// <summary>
    /// The modules that are currently loaded.
    /// </summary>
    /// <remarks>
    /// This collection is not thread safe. Unloading or loading a module will affect this collection.
    /// </remarks>
    public IReadOnlyCollection<ModuleContext> AllModules => _moduleContexts.AsReadOnly();

    /// <summary>
    /// Instantiates a new <see cref="ModuleLoader"/>.
    /// </summary>
    /// <param name="logger">
    /// A logger for the <see cref="ModuleLoader"/>.
    /// </param>
    /// <param name="serviceProvider">
    /// A valid <see cref="IServiceProvider"/> to serve the modules with.
    /// </param>
    public ModuleLoader(
        ILogger<ModuleLoader> logger,
        IServiceProvider serviceProvider)
    {
        _logger                 = logger;
        _serviceProvider        = serviceProvider;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await UnloadAllAsync()
            .ConfigureAwait(false);
        foreach (var moduleContext in _moduleContexts)
        {
            await moduleContext.DisposeAsync()
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Unloads all loaded modules.
    /// </summary>
    public async Task UnloadAllAsync()
    {
        while (_moduleContexts.Any((q) => q.IsLoaded))
        {
            foreach (var moduleContext in _moduleContexts
                         .Where((moduleContext) => moduleContext.IsLoaded)
                         .Where(
                             (moduleContext) => moduleContext.Dependants
                                 .All((dependant) => !dependant.IsLoaded)))
            {
                await moduleContext.UnloadAsync()
                    .ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Loads all unloaded modules.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token to cancel the unloading process at any time.
    /// </param>
    public async Task LoadAllAsync(CancellationToken cancellationToken)
    {
        while (_moduleContexts.Any((q) => !q.IsLoaded))
        {
            foreach (var moduleContext in _moduleContexts
                         .Where((moduleContext) => !moduleContext.IsLoaded)
                         .Where(
                             (moduleContext) => moduleContext.Dependencies
                                 .All((dependency) => dependency.IsLoaded)))
            {
                await moduleContext.LoadModuleAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }


    /// <summary>
    /// Scans the provided <paramref name="moduleDirectories"/> for modules.
    /// </summary>
    /// <remarks>
    /// Every module scan will trigger a re-scan and dependency rebuild, blocking modules from being loaded/unloaded while
    /// the graph is being computed.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <param name="moduleDirectory">The directories to load modules from.</param>
    /// <param name="moduleDirectories">The directories to load modules from.</param>
    public Task ScanForModulesAsync(
        CancellationToken cancellationToken,
        string moduleDirectory,
        params string[] moduleDirectories)
        => ScanForModulesAsync(cancellationToken, moduleDirectories.Prepend(moduleDirectory).ToArray());

    /// <summary>
    /// Scans the provided <paramref name="moduleDirectories"/> for modules.
    /// </summary>
    /// <remarks>
    /// Every module scan will trigger a re-scan and dependency rebuild, blocking modules from being loaded/unloaded while
    /// the graph is being computed.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <param name="moduleDirectories">The directories to load modules from.</param>
    public async Task ScanForModulesAsync(
        CancellationToken cancellationToken,
        string[] moduleDirectories)
    {
        await ModuleLoadSemaphore.LockedAsync(
            async () =>
            {
                await CreateOrUpdateModuleContextsAsync(
                        cancellationToken,
                        _serviceProvider,
                        moduleDirectories)
                    .ConfigureAwait(false);

                RebuildDependencyGraphAsync();
            },
            cancellationToken).ConfigureAwait(false);
    }

    private void RebuildDependencyGraphAsync()
    {
        using var logScope = _logger.BeginScope(nameof(RebuildDependencyGraphAsync));
        _logger.LogInformation("Rebuilding dependency graph of loaded modules");

        MarkAllModuleContextsAsDependenciesUnresolved();
        AddDependenciesToModulesIfNotAdded();
        MarkModulesWithDependenciesResolved();
    }

    private void MarkModulesWithDependenciesResolved()
    {
        foreach (var moduleContext in _moduleContexts)
        {
            var dependenciesResolved = true;
            foreach (var moduleDependency in moduleContext.Configuration.Dependencies)
            {
                if (moduleContext.Dependencies.Any((dependency) => dependency.Guid == moduleDependency.Guid))
                    continue;
                _logger.LogDebug(
                    "Dependency {ModuleDependency} could not be resolved for module {ModuleContext}",
                    moduleDependency.Guid,
                    moduleContext.Guid);
                dependenciesResolved = false;
            }

            moduleContext.AreDependenciesResolved = dependenciesResolved;
        }
    }

    private void AddDependenciesToModulesIfNotAdded()
    {
        foreach (var moduleContext in _moduleContexts)
        {
            foreach (var moduleDependency in moduleContext.Configuration.Dependencies)
            {
                if (moduleContext.Dependencies.Any((q) => q.Guid == moduleContext.Guid))
                {
                    _logger.LogTrace(
                        "Dependency {DependencyGuid} of module context {DependantGuid} is already resolved",
                        moduleDependency.Guid,
                        moduleContext.Guid);
                    continue;
                }

                var dependency = _moduleContexts.FirstOrDefault((q) => q.Guid == moduleDependency.Guid);
                if (dependency is null)
                {
                    _logger.LogTrace(
                        "Failed to locate dependency {DependencyGuid} for module context {DependantGuid}",
                        moduleDependency.Guid,
                        moduleContext.Guid);
                    continue;
                }

                if (dependency.Configuration.Build.Version < moduleDependency.Version)
                {
                    _logger.LogError(
                        "The module {DependantGuid} is depending on a newer version of {DependencyGuid} " +
                        "and hence cannot be resolved",
                        moduleContext.Guid,
                        dependency.Guid);
                    continue;
                }

                _logger.LogDebug(
                    "The module {DependantGuid} is now depending on {DependencyGuid}",
                    moduleContext.Guid,
                    dependency.Guid);
                moduleContext.AddDependency(dependency);
                dependency.AddDependant(moduleContext);
            }
        }
    }

    private void MarkAllModuleContextsAsDependenciesUnresolved()
    {
        foreach (var moduleContext in _moduleContexts)
        {
            moduleContext.AreDependenciesResolved = false;
        }
    }

    private async Task CreateOrUpdateModuleContextsAsync(
        CancellationToken cancellationToken,
        IServiceProvider serviceProvider,
        IEnumerable<string> moduleDirectories)
    {
        using var logScope = _logger.BeginScope(nameof(CreateOrUpdateModuleContextsAsync));
        var moduleContexts = new List<ModuleContext>();
        var moduleConfigLoadExceptions = new List<ModuleConfigLoadingException.Tuple>();
        _moduleDirectories.AddRange(moduleDirectories.Select(Path.GetFullPath));
        foreach (var moduleDirectory in _moduleDirectories)
        {
            _logger.LogDebug("Checking {ModuleDirectory} for module candidates", moduleDirectory);
            var directories = Directory.GetDirectories(moduleDirectory, "*", SearchOption.TopDirectoryOnly);
            foreach (var moduleCandidate in directories)
            {
                if (_moduleContexts.FirstOrDefault(
                        (q) => q.ModuleDirectory == moduleCandidate) is { } moduleContext)
                {
                    await UpdateModuleConfigurationAsync(moduleContext, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    await CreateSingleModuleContextFromAsync(
                            cancellationToken,
                            serviceProvider,
                            moduleCandidate,
                            moduleContexts,
                            moduleConfigLoadExceptions,
                            moduleDirectory)
                        .ConfigureAwait(false);
                }
            }
        }

        if (moduleConfigLoadExceptions.Any())
            throw new ModuleConfigLoadingException(moduleConfigLoadExceptions);
        _moduleContexts.AddRange(moduleContexts);
    }

    private async Task UpdateModuleConfigurationAsync(ModuleContext moduleContext, CancellationToken cancellationToken)
    {
        using var logScope = _logger.BeginScope(nameof(UpdateModuleConfigurationAsync));
        var (config, lastWriteTime) = await ModuleConfiguration.TryLoadAsync(
                moduleContext.ModuleDirectory,
                cancellationToken)
            .ConfigureAwait(false);
        if (config is null)
        {
            _logger.LogError(
                "Failed to load {ConfigFileName} from {ModuleDirectory}",
                ModuleConfiguration.FileName,
                moduleContext.ModuleDirectory);
            throw new LoadingModuleConfigurationFailedException(moduleContext.ModuleDirectory);
        }

        moduleContext.ChangeModuleConfigurationAsync(config, lastWriteTime);
    }

    private async Task CreateSingleModuleContextFromAsync(
        CancellationToken cancellationToken,
        IServiceProvider serviceProvider,
        string moduleCandidate,
        List<ModuleContext> moduleContexts,
        List<ModuleConfigLoadingException.Tuple> moduleConfigLoadExceptions,
        string moduleDirectory)
    {
        using var logScope = _logger.BeginScope(nameof(CreateSingleModuleContextFromAsync));
        var configFile = Path.Combine(moduleCandidate, ModuleConfiguration.FileName);
        if (!File.Exists(configFile))
        {
            _logger.LogDebug(
                "Skipping candidate {ModuleCandidate} as no {ConfigFileName} was found",
                moduleCandidate,
                ModuleConfiguration.FileName);
            return;
        }

        _logger.LogDebug("Attempting to create context for candidate at {ModuleCandidate}", moduleCandidate);
        try
        {
            var moduleContext = await InstantiateModuleContextAsync(
                    serviceProvider,
                    moduleCandidate,
                    cancellationToken)
                .ConfigureAwait(false);

            moduleContexts.Add(moduleContext);
        }
        catch (ModularizationException ex)
        {
            moduleConfigLoadExceptions.Add(new ModuleConfigLoadingException.Tuple(ex, moduleDirectory));
        }
    }

    [Pure]
    private static bool HaveAllDependenciesBeenLoaded(
        ModuleContext moduleContext,
        IReadOnlyCollection<ModuleContext> loadedModules)
    {
        var dependencies = moduleContext.Configuration.Dependencies.Select(
                (dependency) => loadedModules.SingleOrDefault((loadedModule) => loadedModule.Guid == dependency.Guid))
            .NotNull()
            .ToArray();
        if (dependencies.Length != moduleContext.Configuration.Dependencies.Count)
            return false;
        moduleContext.AddDependencies(dependencies);
        foreach (var dependency in dependencies)
        {
            dependency.AddDependant(moduleContext);
        }

        return true;
    }

    [Pure]
    private async Task<ModuleContext> InstantiateModuleContextAsync(
        IServiceProvider serviceProvider,
        string moduleDirectory,
        CancellationToken cancellationToken)
    {
        using var logScope = _logger.BeginScope(nameof(InstantiateModuleContextAsync));
        var (config, lastWriteTime) = await ModuleConfiguration.TryLoadAsync(
                moduleDirectory,
                cancellationToken)
            .ConfigureAwait(false);
        if (config is not null)
            return new ModuleContext(this, serviceProvider, moduleDirectory, config, lastWriteTime);
        _logger.LogError(
            "Failed to load {ConfigFileName} from {ModuleDirectory}",
            ModuleConfiguration.FileName,
            moduleDirectory);
        throw new LoadingModuleConfigurationFailedException(moduleDirectory);
    }
}