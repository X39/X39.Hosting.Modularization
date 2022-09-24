namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a <see cref="Modularization.ModuleContextBase"/> is disposed of while still loaded.
/// </summary>
[PublicAPI]
public class ModuleStillLoadedException : ModularizationException
{
    /// <summary>
    /// The module context that still is loaded.
    /// </summary>
    public ModuleContextBase ModuleContext { get; }
    internal ModuleStillLoadedException(ModuleContextBase moduleContext)
    {
        ModuleContext = moduleContext;
    }
}