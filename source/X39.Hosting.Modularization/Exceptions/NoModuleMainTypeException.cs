namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when no <see cref="Type"/> in a module is implementing <see cref="Abstraction.IModuleMain"/>.
/// </summary>
[PublicAPI]
public class NoModuleMainTypeException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContextBase"/> that is not implementing any <see cref="Type"/>
    /// with <see cref="Abstraction.IModuleMain"/>.
    /// </summary>
    public ModuleContextBase ModuleContext { get; }

    internal NoModuleMainTypeException(ModuleContextBase moduleContext)
    {
        ModuleContext = moduleContext;
    }
}