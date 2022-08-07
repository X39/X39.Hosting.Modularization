namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a <see cref="Modularization.ModuleContext"/> is attempted to be loaded twice. 
/// </summary>
[PublicAPI]
public class ModuleAlreadyLoadedException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> that was loaded twice. 
    /// </summary>
    public ModuleContext ModuleContext { get; }
    internal ModuleAlreadyLoadedException(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}