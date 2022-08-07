using System.Collections.Immutable;

namespace X39.Hosting.Modularization.Exceptions;

/// <summary>
/// Thrown when loading a module failed due to a dependency resolution problem.
/// </summary>
[PublicAPI]
public class DependencyResolutionException : ModularizationException
{
    /// <summary>
    /// The <see cref="ModuleContext"/>s that could not be loaded due to dependency resolution problems.
    /// </summary>
    public IReadOnlyCollection<ModuleContext> ModuleContexts { get; }
    
    internal DependencyResolutionException(IEnumerable<ModuleContext> moduleContexts)
    {
        ModuleContexts = moduleContexts.ToImmutableArray();
    }
}