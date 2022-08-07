namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a <see cref="Modularization.ModuleContext"/> is attempted to be unloaded but is not loaded.
/// </summary>
[PublicAPI]
public class ModuleIsNotLoadedException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> that was not loaded yet. 
    /// </summary>
    public ModuleContext ModuleContext { get; }
    internal ModuleIsNotLoadedException(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}