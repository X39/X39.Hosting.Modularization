namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a <see cref="Type"/> has more then one constructors.
/// </summary>
[PublicAPI]
public class MultipleTypeConstructorsException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> that has the type, implementing
    /// to many constructors.
    /// </summary>
    public ModuleContext ModuleContext { get; }
    
    /// <summary>
    /// The name of the <see cref="Type"/> with multiple constructors.
    /// </summary>
    public string TypeName { get; }
    internal MultipleTypeConstructorsException(ModuleContext moduleContext, string typeName)
    {
        ModuleContext = moduleContext;
        TypeName      = typeName;
    }
}