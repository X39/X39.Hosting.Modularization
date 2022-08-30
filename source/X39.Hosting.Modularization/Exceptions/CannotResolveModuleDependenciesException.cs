using System.Collections.Immutable;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown during instantiation of a <see cref="Type"/> implementing the <see cref="Abstraction.IModuleMain"/>
/// interface when one or more parameters of a constructor, which are marked as non-nullable, could not be resolved
/// using the <see cref="IServiceProvider"/> given to the <see cref="ModuleLoader"/>.
/// </summary>
[PublicAPI]
public class CannotResolveModuleDependenciesException : ModularizationException
{
    /// <summary>
    /// Concrete datatype to group a parameter type <paramref name="Type"/> with a parameter
    /// index <paramref name="Index"/>.
    /// </summary>
    /// <param name="Type">The <see cref="System.Type"/> that could not be resolved.</param>
    /// <param name="Index">The zero based index of the parameter in the constructor.</param>
    public record struct UnresolvedParameter(Type Type, int Index);

    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> that could not be constructed.
    /// </summary>
    public ModuleContext ModuleContext { get; }

    /// <summary>
    /// The parameters which could not be resolved using the <see cref="IServiceProvider"/>.
    /// </summary>
    public IReadOnlyCollection<UnresolvedParameter> UnresolvedParameters { get; }

    internal CannotResolveModuleDependenciesException(
        ModuleContext moduleContext,
        IEnumerable<(int index, Type type)> unresolvedParameters)
    {
        ModuleContext = moduleContext;
        UnresolvedParameters = unresolvedParameters
            .Select((q) => new UnresolvedParameter(q.type, q.index))
            .ToImmutableArray();
    }
}