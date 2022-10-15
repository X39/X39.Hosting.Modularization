namespace X39.Hosting.Modularization.Abstraction;

/// <summary>
/// Injectable <see cref="Guid"/> for the module.
/// </summary>
/// <remarks>
/// Allows to not refer to the whole modularization project but being able to receive the module <see cref="Guid"/>.
/// </remarks>
/// <param name="Value">The modules <see cref="Guid"/> as set in the modules config.</param>
public sealed record ModuleGuid(Guid Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="ModuleGuid"/> to a <see cref="Guid"/>.
    /// </summary>
    /// <param name="self">The <see cref="ModuleGuid"/> to get the <see cref="Value"/> from.</param>
    /// <returns>The <see cref="ModuleGuid.Value"/></returns>
    public static implicit operator Guid(ModuleGuid self) => self.Value;

    /// <summary>
    /// Implicitly converts a <see cref="Guid"/> to a <see cref="ModuleGuid"/>.
    /// </summary>
    /// <param name="self">The <see cref="Guid"/> to create a new <see cref="ModuleGuid"/> from.</param>
    /// <returns>The <see cref="ModuleGuid"/> class created.</returns>
    public static implicit operator ModuleGuid(Guid self) => new(self);
}