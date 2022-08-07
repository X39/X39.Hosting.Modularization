namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a <see cref="Modularization.ModuleContext"/> is disposed of while still loaded.
/// </summary>
[PublicAPI]
public class ModuleStillLoadedException : ModularizationException
{
    /// <summary>
    /// The module context that still is loaded.
    /// </summary>
    public ModuleContext ModuleContext { get; }
    internal ModuleStillLoadedException(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}