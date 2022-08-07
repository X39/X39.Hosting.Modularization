namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a <see cref="Modularization.ModuleContext"/> is attempted to be accessed
/// before it has been loaded fully.
/// </summary>
/// <remarks>
/// One reasons this exception can be thrown is if a <see cref="Modularization.ModuleContext"/> is attempted
/// to be unloaded before it has been fully loaded.
/// </remarks>
[PublicAPI]
public class ModuleLoadingNotFinishedException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> that is still loading. 
    /// </summary>
    public ModuleContext ModuleContext { get; }
    internal ModuleLoadingNotFinishedException(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}