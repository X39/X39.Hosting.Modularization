namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a module main <see cref="Type"/> has more then one constructors.
/// </summary>
[PublicAPI]
public class MultipleMainTypeConstructorsException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> that has the type, implementing
    /// to many constructors.
    /// </summary>
    public ModuleContext ModuleContext { get; }
    
    /// <summary>
    /// The name of the <see cref="Type"/> resolved as the module main.
    /// </summary>
    public string TypeName { get; }
    internal MultipleMainTypeConstructorsException(ModuleContext moduleContext, string typeName)
    {
        ModuleContext = moduleContext;
        TypeName      = typeName;
    }
}