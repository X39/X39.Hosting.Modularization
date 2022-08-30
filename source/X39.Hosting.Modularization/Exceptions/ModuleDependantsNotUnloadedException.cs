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
    /// The <see cref="Modularization.ModuleContext"/> that was attempted to be unloaded.
    /// </summary>
    public ModuleContext ModuleContext { get; }

    /// <summary>
    /// The dependants which are still loaded.
    /// </summary>
    public IReadOnlyCollection<ModuleContext> LoadedDependants { get; }

    internal ModuleDependantsNotUnloadedException(
        ModuleContext moduleContext,
        IEnumerable<ModuleContext> loadedDependants)
        : base($"The module {moduleContext.Guid} cannot be unloaded one or more dependants are still loaded.")
    {
        ModuleContext    = moduleContext;
        LoadedDependants = loadedDependants.ToImmutableArray();
    }
}