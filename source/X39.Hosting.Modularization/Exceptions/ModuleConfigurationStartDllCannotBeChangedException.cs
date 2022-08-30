namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a <see cref="Modularization.ModuleContext"/>'s config was altered and the start-dll
/// value was changed.
/// </summary>
[PublicAPI]
public class ModuleConfigurationStartDllCannotBeChangedException : ModuleConfigurationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> where the start dll config has changed.
    /// </summary>
    public ModuleContext ModuleContext { get; }
    internal ModuleConfigurationStartDllCannotBeChangedException(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}