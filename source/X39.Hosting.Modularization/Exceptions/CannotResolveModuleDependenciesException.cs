namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown during instantiation of a <see cref="Type"/> implementing the <see cref="Abstraction.IModuleMain"/>
/// interface when one or more parameters of a constructor, which are marked as non-nullable, could not be resolved
/// using the <see cref="IServiceProvider"/> given to the <see cref="ModuleLoader"/>.
/// </summary>
[PublicAPI]
public class CannotResolveModuleDependenciesException : CannotResolveDependenciesException
{
    internal CannotResolveModuleDependenciesException(
        ModuleContextBase moduleContext,
        IEnumerable<(int index, Type type)> unresolvedParameters)
        : base(moduleContext, unresolvedParameters)
    {
    }
}