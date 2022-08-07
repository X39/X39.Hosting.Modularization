using System.Collections.Immutable;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Aggregates one or more exceptions that occurred during the loading of a module context.
/// </summary>
/// <remarks>
/// When thrown, some modules may already have been loaded. Modules which are part of <see cref="InnerExceptions"/>
/// are guaranteed to be either not loaded at all or unloaded immediately.
/// </remarks>
[PublicAPI]
public class ModuleContextLoadingException : ModularizationException
{
    /// <summary>
    /// A tuple containing the module directory and the exception that occurred.
    /// </summary>
    /// <param name="Exception">The exception that occured while loading the modules config.</param>
    /// <param name="ModuleContext">The <see cref="ModuleContext"/> that failed to load</param>
    public record Tuple(Exception Exception, ModuleContext ModuleContext);
    /// <summary>
    /// Tuples containing the <see cref="ModuleContext"/> and the exception that occurred.
    /// </summary>
    public IReadOnlyCollection<Tuple> InnerExceptions { get; }
    internal ModuleContextLoadingException(IEnumerable<Tuple> tuples)
    {
        InnerExceptions = tuples.ToImmutableArray();
    }
}