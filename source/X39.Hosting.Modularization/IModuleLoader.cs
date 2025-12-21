using X39.Hosting.Modularization.Abstraction;
using X39.Hosting.Modularization.Configuration;
using X39.Hosting.Modularization.Data;

namespace X39.Hosting.Modularization;

/// <summary>
/// Represents the contract for a module loader in a modular application,
/// providing methods for module discovery, loading, unloading, and event handling for module lifecycle events.
/// </summary>
[PublicAPI]
public interface IModuleLoader
{
    /// <summary>
    /// Raised when a <see cref="ModuleContextBase"/> created by this <see cref="ModuleLoader"/> is unloading.
    /// </summary>
    event AsyncEventHandler<UnloadingEventArgs>? ModuleUnloading;

    /// <summary>
    /// Raised when a <see cref="ModuleContextBase"/> created by this <see cref="ModuleLoader"/> has finished unloading.
    /// </summary>
    event AsyncEventHandler<UnloadedEventArgs>? ModuleUnloaded;

    /// <summary>
    /// Raised when a <see cref="ModuleContextBase"/> created by this <see cref="ModuleLoader"/> is being loaded.
    /// </summary>
    event AsyncEventHandler<LoadingEventArgs>? ModuleLoading;

    /// <summary>
    /// Raised when a <see cref="ModuleContextBase"/> created by this <see cref="ModuleLoader"/>
    /// has loaded the <see cref="System.Reflection.Assembly"/>.
    /// </summary>
    event AsyncEventHandler<AssemblyLoadedEventArgs>? ModuleAssemblyLoaded;

    /// <summary>
    /// Raised when a <see cref="ModuleContextBase"/> created by this <see cref="ModuleLoader"/> has finished loading.
    /// </summary>
    event AsyncEventHandler<LoadedEventArgs>? ModuleLoaded;

    /// <summary>
    /// Raised when a <see cref="ModuleContextBase"/> was created.
    /// </summary>
    event AsyncEventHandler<DiscoveredEventArgs>? ModuleDiscovered;

    /// <summary>
    /// The modules that are currently loaded.
    /// </summary>
    /// <remarks>
    /// This collection is not thread safe. Unloading or loading a module will affect this collection.
    /// </remarks>
    IReadOnlyCollection<ModuleContextBase> AllModules { get; }

    /// <summary>
    /// Unloads all loaded modules.
    /// </summary>
    Task UnloadAllAsync();

    /// <summary>
    /// Loads all unloaded modules.
    /// </summary>
    /// <param name="cancellationToken">
    /// A cancellation token to cancel the unloading process at any time.
    /// </param>
    Task LoadAllAsync(CancellationToken cancellationToken);

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
    Task ScanForModulesAsync(
        CancellationToken cancellationToken,
        string moduleDirectory,
        params string[] moduleDirectories);

    /// <summary>
    /// Scans the provided <paramref name="moduleDirectories"/> for modules.
    /// </summary>
    /// <remarks>
    /// Every module scan will trigger a re-scan and dependency rebuild, blocking modules from being loaded/unloaded while
    /// the graph is being computed.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <param name="moduleDirectories">The directories to load modules from.</param>
    Task ScanForModulesAsync(
        CancellationToken cancellationToken,
        string[] moduleDirectories);

    /// <summary>
    /// Allows to add a managed module to the list of available modules.
    /// A managed module acts as if it is an external module regarding
    /// lifetime guarantees but the assembly lifetime is managed externally.
    /// </summary>
    /// <param name="moduleConfiguration">The <see cref="ModuleConfiguration"/> of the module.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
    /// <typeparam name="TModuleMain">The type of the main-module to add.</typeparam>
    void AddManagedModule<TModuleMain>(
        ModuleConfiguration moduleConfiguration,
        CancellationToken cancellationToken)
        where TModuleMain : IModuleMain;
}
