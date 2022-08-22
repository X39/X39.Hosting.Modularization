using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using X39.Hosting.Modularization.Configuration;
using X39.Hosting.Modularization.Exceptions;
using X39.Util.Collections;

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
    private readonly List<ModuleContext>   _foundModuleContexts = new();

    /// <summary>
    /// The modules that are currently loaded.
    /// </summary>
    /// <remarks>
    /// This collection is not thread safe. Unloading or loading a module will affect this collection.
    /// </remarks>
    public IReadOnlyCollection<ModuleContext> AllModules => _foundModuleContexts.AsReadOnly();

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
        await UnloadAllAsync()
            .ConfigureAwait(false);
        foreach (var moduleContext in _foundModuleContexts)
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
        while (_foundModuleContexts.Any((q) => q.IsLoaded))
        {
            foreach (var moduleContext in _foundModuleContexts
                         .Where((moduleContext) => moduleContext.IsLoaded)
                         .Where((moduleContext) => moduleContext.Dependants
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
        while (_foundModuleContexts.Any((q) => !q.IsLoaded))
        {
            foreach (var moduleContext in _foundModuleContexts
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
    /// Loads the modules at <paramref name="moduleDirectories"/>.
    /// </summary>
    /// <remarks>
    /// For module dependencies to be resolved correctly during startup,
    /// this method should be called only once with all modules supposed to be loaded.
    /// Otherwise, the dependencies of the modules will not be resolved correctly resulting in an exception.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <param name="moduleDirectories">The directories to load modules from.</param>
    public async Task PrepareModulesInAsync(
        CancellationToken cancellationToken,
        params string[] moduleDirectories)
    {
        var result = await CreateMultipleModuleContextsAsync(
                cancellationToken,
                _serviceProvider,
                moduleDirectories)
            .ConfigureAwait(false);
        var moduleContexts = result.ToList();

        await PrepareDependenciesAsync(moduleContexts, cancellationToken)
            .ConfigureAwait(false);
    }

    private Task PrepareDependenciesAsync(
        IEnumerable<ModuleContext> moduleContexts,
        CancellationToken cancellationToken)
    {
        var lastCount = 0;
        var pendingModuleContexts = moduleContexts.ToList();
        while (pendingModuleContexts.Any() && lastCount != pendingModuleContexts.Count)
        {
            lastCount = pendingModuleContexts.Count;
            foreach (var moduleContext in pendingModuleContexts
                         .Where(moduleContext => HaveAllDependenciesBeenLoaded(moduleContext, _foundModuleContexts))
                         .ToImmutableArray())
            {
                _logger.LogInformation(
                    "Module {StartDll} ({ModuleName}) successfully prepared for loading",
                    moduleContext.Configuration.StartDll,
                    moduleContext.Configuration.Guid);
                _foundModuleContexts.Add(moduleContext);
                pendingModuleContexts.Remove(moduleContext);
            }
        }

        if (pendingModuleContexts.Any())
            throw new DependencyResolutionException(pendingModuleContexts);
        return Task.CompletedTask;
    }

    private async Task<IEnumerable<ModuleContext>> CreateMultipleModuleContextsAsync(
        CancellationToken cancellationToken,
        IServiceProvider serviceProvider,
        IEnumerable<string> moduleDirectories)
    {
        using var logScope = _logger.BeginScope(nameof(CreateMultipleModuleContextsAsync));
        var moduleContexts = new List<ModuleContext>();
        var moduleConfigLoadExceptions = new List<ModuleConfigLoadingException.Tuple>();
        foreach (var moduleDirectory in moduleDirectories.Select(Path.GetFullPath))
        {
            _logger.LogDebug("Checking {ModuleDirectory} for module candidates", moduleDirectory);
            var directories = Directory.GetDirectories(moduleDirectory, "*", SearchOption.TopDirectoryOnly);
            foreach (var moduleCandidate in directories)
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

        if (moduleConfigLoadExceptions.Any())
            throw new ModuleConfigLoadingException(moduleConfigLoadExceptions);
        return moduleContexts;
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
            dependency.AddDependants(moduleContext);
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
        var config = await Configuration.ModuleConfiguration.TryLoadAsync(
                moduleDirectory,
                cancellationToken)
            .ConfigureAwait(false);
        if (config is not null)
            return new ModuleContext(serviceProvider, moduleDirectory, config);
        _logger.LogError(
            "Failed to load {ConfigFileName} from {ModuleDirectory}",
            ModuleConfiguration.FileName,
            moduleDirectory);
        throw new LoadingModuleConfigurationFailedException(moduleDirectory);
    }
}