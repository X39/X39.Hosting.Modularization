namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when the <see cref="Type"/> implementing the <see cref="Abstraction.IModuleMain"/> interface
/// is also a generic one (eg. ModuleMain&lt;T&gt;).
/// </summary>
[PublicAPI]
public class ModuleMainTypeIsGenericException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContextBase"/> that has the type, implementing
    /// <see cref="Abstraction.IModuleMain"/> on a generic type.
    /// </summary>
    public ModuleContextBase ModuleContext { get; }
    
    /// <summary>
    /// The name of the <see cref="Type"/> in question.
    /// </summary>
    public string TypeName { get; }
    internal ModuleMainTypeIsGenericException(ModuleContextBase moduleContext, string typeName)
    {
        ModuleContext = moduleContext;
        TypeName      = typeName;
    }
}