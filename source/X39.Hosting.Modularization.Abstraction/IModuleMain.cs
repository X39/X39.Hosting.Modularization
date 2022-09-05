using Microsoft.Extensions.DependencyInjection;

namespace X39.Hosting.Modularization.Abstraction;

/// <summary>
///     <para>
///         Defines a <see langword="class"/> as a startup for a module.
///         The startup <see langword="class"/> will be instantiated once and kept alive for the whole lifetime
///         of the module.
///     </para>
///     <para>
///         Lifetime begins when the module is loaded and ends when the module is unloaded.
///         Lifetime also ends when the initializer function, provided by <see cref="ConfigureAsync"/>,
///         is throwing an exception while being executed (will also call dispose functions if applicable).
///     </para>
///     <para>
///         A module may receive its ModuleContext if any parameter is of the module context type, requiring a strong
///         reference to the X39.Hosting.Modularization package (the package containing the type).
///         Nullability of parameters is enforced by the module loader.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         Rules for module mains:
///         <list type="bullet">
///             <item>Only one <see langword="class"/> in a modules assembly may be marked with this attribute.</item>
///             <item>
///                 The implementing <see langword="class"/> may be <see langword="abstract"/>,
///                 given there exists a non-<see langword="abstract"/> derivative.
///             </item>
///             <item>The module main <see langword="class"/> may only have up to one constructor.</item>
///             <item>A module main <see langword="class"/> may not be a generic (eg. MyModule&lt;TSomething&gt;)</item>
///         </list>
///     </para>
///     <para>
///         Recommendations:
///         <list type="bullet">
///             <item>
///                 It is recommended to enabled nullability as the framework uses the hints to throw an error
///                 if a dependency could not be resolved via dependency injection.
///             </item>
///             <item>It is recommended to place a <see cref="UsedImplicitlyAttribute"/> on the class.</item>
///             <item>It is recommended to make the constructor private to prevent accidental creation.</item>
///             <item>It is recommended to make the startup <see langword="class"/> <see langword="sealed"/>.</item>
///             <item>It is recommended to implement the methods in <see cref="IModuleMain"/> explicitly.</item>
///         </list>
///     </para>
/// </remarks>
[PublicAPI]
public interface IModuleMain : IAsyncDisposable
{

    /// <summary>
    /// Called to set up the services provided by this module, prior to <see cref="ConfigureAsync"/>.
    /// </summary>
    /// <param name="serviceCollection">A service collection to add services to.</param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> to stop the initialization, ending the lifetime of the module immediately.
    /// </param>
    /// <returns>An awaitable <see cref="ValueTask"/>.</returns>
    ValueTask ConfigureServicesAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken);
    /// <summary>
    /// Called when the module is loaded and after <see cref="ConfigureServicesAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken"/> to stop the initialization, ending the lifetime of the module immediately.
    /// </param>
    /// <returns>An awaitable <see cref="ValueTask"/>.</returns>
    ValueTask ConfigureAsync(CancellationToken cancellationToken);
}