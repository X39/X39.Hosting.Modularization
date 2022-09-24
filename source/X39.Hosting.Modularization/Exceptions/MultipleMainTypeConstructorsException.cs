namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when a module main <see cref="Type"/> has more then one constructors.
/// </summary>
[PublicAPI]
public class MultipleMainTypeConstructorsException : MultipleTypeConstructorsException
{
    internal MultipleMainTypeConstructorsException(ModuleContextBase moduleContext, string typeName) : base(moduleContext, typeName)
    {
    }
}