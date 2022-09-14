using System.Collections.Immutable;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown during instantiation of a <see cref="Type"/> when one or more parameters of a constructor,
/// which are marked as non-nullable, could not be resolved.
/// </summary>
[PublicAPI]
public class CannotResolveDependenciesException : ModularizationException
{
    /// <summary>
    /// Concrete datatype to group a parameter type <paramref name="Type"/> with a parameter
    /// index <paramref name="Index"/>.
    /// </summary>
    /// <param name="Type">The <see cref="System.Type"/> that could not be resolved.</param>
    /// <param name="Index">The zero based index of the parameter in the constructor.</param>
    public record struct UnresolvedParameter(Type Type, int Index);

    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> used during construction..
    /// </summary>
    public ModuleContext ModuleContext { get; }

    /// <summary>
    /// The parameters which could not be resolved using the <see cref="IServiceProvider"/>.
    /// </summary>
    public IReadOnlyCollection<UnresolvedParameter> UnresolvedParameters { get; }

    internal CannotResolveDependenciesException(
        ModuleContext moduleContext,
        IEnumerable<(int index, Type type)> unresolvedParameters)
    {
        ModuleContext = moduleContext;
        UnresolvedParameters = unresolvedParameters
            .Select((q) => new UnresolvedParameter(q.type, q.index))
            .ToImmutableArray();
    }
}