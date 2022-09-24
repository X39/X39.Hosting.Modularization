using System.Collections.Immutable;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a module is attempted to be unloaded while one or more modules, depending on the one being unloaded,
/// are still loaded.
/// To solve this, unload the dependants first.
/// </summary>
[PublicAPI]
public class ModuleDependantsNotUnloadedException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContextBase"/> that was attempted to be unloaded.
    /// </summary>
    public ModuleContextBase ModuleContext { get; }

    /// <summary>
    /// The dependants which are still loaded.
    /// </summary>
    public IReadOnlyCollection<ModuleContextBase> LoadedDependants { get; }

    internal ModuleDependantsNotUnloadedException(
        ModuleContextBase moduleContext,
        IEnumerable<ModuleContextBase> loadedDependants)
        : base($"The module {moduleContext.Guid} cannot be unloaded one or more dependants are still loaded.")
    {
        ModuleContext    = moduleContext;
        LoadedDependants = loadedDependants.ToImmutableArray();
    }
}