namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Exception thrown when the configuration of a module is not found.
/// </summary>
[PublicAPI]
public class LoadingModuleConfigurationFailedException : ModularizationException
{
    /// <summary>
    /// The directory where the module was supposed to be located in.
    /// </summary>
    public string ModuleDirectory { get; }
    internal LoadingModuleConfigurationFailedException(string moduleDirectory)
    {
        ModuleDirectory = moduleDirectory;
    }
}