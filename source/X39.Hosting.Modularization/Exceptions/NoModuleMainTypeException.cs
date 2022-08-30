namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when no <see cref="Type"/> in a module is implementing <see cref="Abstraction.IModuleMain"/>.
/// </summary>
[PublicAPI]
public class NoModuleMainTypeException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> that is not implementing any <see cref="Type"/>
    /// with <see cref="Abstraction.IModuleMain"/>.
    /// </summary>
    public ModuleContext ModuleContext { get; }

    internal NoModuleMainTypeException(ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
    }
}