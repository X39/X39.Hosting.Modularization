namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when the configuration of a loaded <see cref="Modularization.ModuleContext"/> was changed and the
/// build version updated.
/// </summary>
/// <remarks>
/// Build version may only be changed when a module is not loaded as a build version change would have to involve
/// a change of the assemblies involved.
/// </remarks>
[PublicAPI]
public class ModuleConfigurationBuildVersionCannotBeChangedException : ModuleConfigurationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> where the build version listed in the config has changed.
    /// </summary>
    public ModuleContext ModuleContext { get; }
    internal ModuleConfigurationBuildVersionCannotBeChangedException(ModuleContext moduleContext)
        : base($"The configuration of {moduleContext.Guid} has changed and the build version was updated. " +
               $"This is not allowed while the module is loaded.")
    {
        ModuleContext = moduleContext;
    }
}