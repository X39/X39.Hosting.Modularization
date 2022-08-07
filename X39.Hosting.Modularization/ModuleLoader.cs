using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Exceptions;
using Exception = System.Exception;

namespace X39.Hosting.Modularization;

public class ModuleLoader : IAsyncDisposable
{
    private readonly IServiceProvider      _serviceProvider;
    private readonly ILogger<ModuleLoader> _logger;
    private readonly List<ModuleContext>   _loadedModules = new();
    public IReadOnlyCollection<ModuleContext> LoadedModules => _loadedModules.AsReadOnly();

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
    }


    /// <summary>
    /// Loads the modules at <paramref name="moduleDirectories"/>.
    /// </summary>
    /// <remarks>
    /// This call should only be made once, in the application's startup!
    /// Multiple calls will not properly resolve module-based dependencies.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <param name="serviceProvider">Service provider to use for module resolution.</param>
    /// <param name="moduleDirectories">The directories to load modules from.</param>
    public async Task LoadModulesAsync(
        CancellationToken cancellationToken,
        IServiceProvider serviceProvider,
        params string[] moduleDirectories)
    {
        var moduleContexts = new List<ModuleContext>();
        var exceptions = new List<Exception>();
        foreach (var moduleDirectory in moduleDirectories)
        {
            try
            {
                var moduleContext = await CreateModuleContextAsync(serviceProvider, moduleDirectory, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (ModularizationException ex)
            {
                exceptions.Add(ex);
            }
        }

        var lastCount = 0;
        while (moduleContexts.Any() && lastCount != moduleContexts.Count)
        {
            foreach (var moduleContext in moduleContexts)
            {
                if (!AreAllDependenciesLoaded(moduleContext, _loadedModules))
                    continue;
                _logger.LogInformation(
                    "Loading module {StartDll} ({ModuleName})",
                    moduleContext.Configuration.StartDll,
                    moduleContext.Configuration.Guid);
                await moduleContext.LoadModuleAsync(_logger, moduleContext);
            }
        }
    }

    private static bool AreAllDependenciesLoaded(ModuleContext moduleContext, IReadOnlyCollection<ModuleContext> loadedModules)
    {
        return moduleContext.Configuration.Dependencies.All(
            (guid) => loadedModules.Any((loadedModule) => loadedModule.Guid == guid));
    }

    [Pure]
    private async Task<ModuleContext?> CreateModuleContextAsync(
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