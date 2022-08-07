using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Exceptions;

namespace X39.Hosting.Modularization;

/// <summary>
/// The modularization host class.
/// It is the main entry point for the modularization system, offering support for loading and unloading independent
/// software modules during runtime.
/// </summary>
[PublicAPI]
public sealed class ModuleLoader : IAsyncDisposable
{
    private readonly IServiceProvider      _serviceProvider;
    private readonly ILogger<ModuleLoader> _logger;
    private readonly List<ModuleContext>   _loadedModules = new();

    /// <summary>
    /// The modules that are currently loaded.
    /// </summary>
    /// <remarks>
    /// This collection is not thread safe. Unloading or loading a module will affect this collection.
    /// </remarks>
    public IReadOnlyCollection<ModuleContext> LoadedModules => _loadedModules.AsReadOnly();

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
        _logger          = logger;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var moduleContext in _loadedModules)
        {
            await moduleContext.UnloadModuleAsync();
            await moduleContext.DisposeAsync();
        }
    }


    /// <summary>
    /// Loads the modules at <paramref name="moduleDirectories"/>.
    /// </summary>
    /// <remarks>
    /// For module dependencies to be resolved correctly during startup,
    /// this method should be called only once with all modules supposed to be loaded.
    /// Otherwise, the dependencies of the modules will not be resolved correctly resulting in an exception.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <param name="serviceProvider">Service provider to use for module resolution.</param>
    /// <param name="moduleDirectories">The directories to load modules from.</param>
    public async Task LoadModulesAsync(
        CancellationToken cancellationToken,
        IServiceProvider serviceProvider,
        params string[] moduleDirectories)
    {
        var moduleContexts = (await CreateModuleContextsAsync(
                cancellationToken,
                serviceProvider,
                moduleDirectories))
            .ToList();

        await LoadModulesAsync(moduleContexts, cancellationToken);
    }

    private async Task LoadModulesAsync(IEnumerable<ModuleContext> moduleContexts, CancellationToken cancellationToken)
    {
        var lastCount = 0;
        var moduleInitializationExceptions = new List<ModuleContextLoadingException.Tuple>();
        var pendingModuleContexts = moduleContexts.ToList();
        while (pendingModuleContexts.Any() && lastCount != pendingModuleContexts.Count)
        {
            foreach (var moduleContext in pendingModuleContexts
                         .Where(moduleContext => AreAllDependenciesLoaded(moduleContext, _loadedModules)))
            {
                _logger.LogInformation(
                    "Loading module {StartDll} ({ModuleName})",
                    moduleContext.Configuration.StartDll,
                    moduleContext.Configuration.Guid);
                try
                {
                    await moduleContext.LoadModuleAsync(moduleContext, cancellationToken);
                    _loadedModules.Add(moduleContext);
                    pendingModuleContexts.Remove(moduleContext);
                    _logger.LogDebug(
                        "Module {StartDll} ({ModuleName}) was loaded successfully",
                        moduleContext.Configuration.StartDll,
                        moduleContext.Configuration.Guid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed loading module {StartDll} ({ModuleName})",
                        moduleContext.Configuration.StartDll,
                        moduleContext.Configuration.Guid);
                    moduleInitializationExceptions.Add(new ModuleContextLoadingException.Tuple(ex, moduleContext));
                }
            }

            lastCount = pendingModuleContexts.Count;
        }

        if (moduleInitializationExceptions.Any())
            throw new ModuleContextLoadingException(moduleInitializationExceptions);

        if (pendingModuleContexts.Any())
            throw new DependencyResolutionException(pendingModuleContexts);
    }

    private async Task<IEnumerable<ModuleContext>> CreateModuleContextsAsync(
        CancellationToken cancellationToken,
        IServiceProvider serviceProvider,
        IEnumerable<string> moduleDirectories)
    {
        var moduleContexts = new List<ModuleContext>();
        var moduleConfigLoadExceptions = new List<ModuleConfigLoadingException.Tuple>();
        foreach (var moduleDirectory in moduleDirectories)
        {
            try
            {
                var moduleContext = await CreateModuleContextAsync(serviceProvider, moduleDirectory, cancellationToken)
                    .ConfigureAwait(false);

                moduleContexts.Add(moduleContext);
            }
            catch (ModularizationException ex)
            {
                moduleConfigLoadExceptions.Add(new ModuleConfigLoadingException.Tuple(ex, moduleDirectory));
            }
        }

        if (moduleConfigLoadExceptions.Any())
            throw new ModuleConfigLoadingException(moduleConfigLoadExceptions);
        return moduleContexts;
    }

    private static bool AreAllDependenciesLoaded(
        ModuleContext moduleContext,
        IReadOnlyCollection<ModuleContext> loadedModules)
    {
        return moduleContext.Configuration.Dependencies.All(
            (guid) => loadedModules.Any((loadedModule) => loadedModule.Guid == guid));
    }

    [Pure]
    private async Task<ModuleContext> CreateModuleContextAsync(
        IServiceProvider serviceProvider,
        string moduleDirectory,
        CancellationToken cancellationToken)
    {
        var config = await Configuration.ModuleConfiguration.TryLoadAsync(
                moduleDirectory,
                cancellationToken)
            .ConfigureAwait(false);
        if (config is null)
        {
            _logger.LogError("Failed to load module configuration from {ModuleDirectory}", moduleDirectory);
            throw new LoadingModuleConfigurationFailedException(moduleDirectory);
        }

        return new ModuleContext(serviceProvider, moduleDirectory, config);
    }
}