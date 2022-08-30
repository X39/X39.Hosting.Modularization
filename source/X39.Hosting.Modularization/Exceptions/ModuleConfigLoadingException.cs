using System.Collections.Immutable;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Aggregates one or more exceptions that occurred during the loading of a module config.
/// </summary>
/// <remarks>
/// When thrown, no module was loaded by the call.
/// </remarks>
[PublicAPI]
public class ModuleConfigLoadingException : ModuleConfigurationException
{
    /// <summary>
    /// A tuple containing the module directory and the exception that occurred.
    /// </summary>
    /// <param name="Exception">The exception that occured while loading the modules config.</param>
    /// <param name="Directory">The directory of the module</param>
    public record Tuple(Exception Exception, string Directory);
    /// <summary>
    /// Tuples containing the module directory and the exception that occurred.
    /// </summary>
    /// <remarks>
    /// Due to the loading state of the module, only the directory of the module is available.
    /// </remarks>
    public IReadOnlyCollection<Tuple> InnerExceptions { get; }
    internal ModuleConfigLoadingException(IEnumerable<Tuple> tuples)
    {
        InnerExceptions = tuples.ToImmutableArray();
    }
}