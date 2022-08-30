namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when module dependencies are rebuild and the listed dependencies in the config of a module changed.
/// </summary>
[PublicAPI]
public class ModuleConfigurationDependenciesChangedForLoadedModuleException : ModuleConfigurationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> where the dependencies listed in the config have changed.
    /// </summary>
    public ModuleContext ModuleContext { get; }

    internal ModuleConfigurationDependenciesChangedForLoadedModuleException(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}