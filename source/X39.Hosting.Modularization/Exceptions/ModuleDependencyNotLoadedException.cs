using System.Collections.Immutable;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a module is attempted to be loaded without all dependencies loaded first,
/// To solve this, load the dependencies first.
/// </summary>
[PublicAPI]
public class ModuleDependencyNotLoadedException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContextBase"/> that was attempted to be loaded.
    /// </summary>
    public ModuleContextBase ModuleContext { get; }

    /// <summary>
    /// The dependants which are not loaded.
    /// </summary>
    public IReadOnlyCollection<ModuleContextBase> UnloadedDependencies { get; }

    internal ModuleDependencyNotLoadedException(
        ModuleContextBase moduleContext,
        IEnumerable<ModuleContextBase> unloadedDependencies)
        : base($"Cannot load module {moduleContext.Guid} as one or more dependencies are not loaded yet.")
    {
        ModuleContext        = moduleContext;
        UnloadedDependencies = unloadedDependencies.ToImmutableArray();
    }
}