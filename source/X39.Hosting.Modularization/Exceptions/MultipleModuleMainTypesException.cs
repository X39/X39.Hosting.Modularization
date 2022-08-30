using System.Collections.Immutable;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when more then a single <see cref="Type"/> in a module implements <see cref="Abstraction.IModuleMain"/>.
/// </summary>
[PublicAPI]
public class MultipleModuleMainTypesException : ModularizationException
{
    /// <summary>
    /// The <see cref="Modularization.ModuleContext"/> that has more then one module main <see cref="Type"/> candidates.
    /// </summary>
    public ModuleContext ModuleContext { get; }

    /// <summary>
    /// The name of every <see cref="Type"/> that implements <see cref="Abstraction.IModuleMain"/>.
    /// </summary>
    public IReadOnlyCollection<string> TypeNames { get; }

    internal MultipleModuleMainTypesException(ModuleContext moduleContext, IEnumerable<string> typeNames)
    {
        ModuleContext = moduleContext;
        TypeNames     = typeNames.ToImmutableArray();
    }
}