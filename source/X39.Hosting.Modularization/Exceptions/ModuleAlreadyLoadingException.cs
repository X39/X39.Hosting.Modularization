namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a <see cref="Modularization.ModuleContext"/> is already loading. 
/// </summary>
[PublicAPI]
public class ModuleAlreadyLoadingException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> that is already loading. 
    /// </summary>
    public ModuleContext ModuleContext { get; }
    internal ModuleAlreadyLoadingException(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}