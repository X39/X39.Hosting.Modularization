namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when the configuration of a <see cref="Modularization.ModuleContext"/> was altered and
/// the guid was changed.
/// </summary>
[PublicAPI]
public class ModuleConfigurationGuidCannotBeChangedException : ModuleConfigurationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> where the guid in the config has changed.
    /// </summary>
    public ModuleContext ModuleContext { get; }

    internal ModuleConfigurationGuidCannotBeChangedException(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}