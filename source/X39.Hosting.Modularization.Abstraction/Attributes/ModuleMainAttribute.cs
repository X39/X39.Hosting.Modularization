namespace X39.Hosting.Modularization.Abstraction.Attributes;

/// <summary>
/// <para>
/// Defines a <see langword="class"/> as a startup for a module.
/// The startup <see langword="class"/> will be instantiated once and kept alive for the whole lifetime of the module.
/// Modules implementing either the <see cref="IDisposable"/> or <see cref="IAsyncDisposable"/> interface will
/// be disposed when the module is unloaded.
/// </para>
/// <para>
/// Lifetime begins when the module is loaded and ends when the module is unloaded.
/// Lifetime also ends when the optional initializer function, provided by the <see cref="InitializeFunction"/>,
/// is throwing an exception while being executed (will also call dispose functions if applicable).
/// </para>
/// <para>
/// A module may receive its ModuleContext if any parameter is of the module context type, requiring a strong
/// reference to the X39.Hosting.Modularization package.
/// Nullability of parameters is enforced by the module loader.
/// </para>
/// </summary>
/// <remarks>
/// Rules for module mains:
/// <list type="bullet">
/// <item>
/// Only one <see langword="class"/> in a modules assembly may be marked with this attribute.
/// It is recommended to make the startup <see langword="class"/> <see langword="sealed"/>.
/// </item>
/// <item>A module-main class may only contain one constructor</item>
/// <item>A module-main class may not be a generic (eg. MyModule&lt;TSomething&gt;)</item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
[PublicAPI]
public class ModuleMainAttribute : Attribute
{
    /// <summary>
    /// The name of a function in the module-main class that will be called when the module is loaded.
    /// </summary>
    /// <remarks>
    /// The function must be an instance function returning a <see cref="Task"/>
    /// and accept a <see cref="CancellationToken"/>:
    /// <code>
    /// Task InitializeAsync(CancellationToken cancellationToken)
    /// </code>
    ///
    /// It is recommended to scope it private to avoid accidentally calling it from other classes.
    /// </remarks>
    public string? InitializeFunction { get; set; }
}