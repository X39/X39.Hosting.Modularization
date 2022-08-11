using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace X39.Hosting.Modularization.Configuration;

/// <summary>
/// A class representing a module dependency.
/// </summary>
[PublicAPI]
public record ModuleDependency
{
    /// <summary>
    /// The <see cref="Guid"/> that uniquely identifies the dependency module.
    /// </summary>
    [JsonPropertyName("uuid"), Required]
    public Guid Guid { get; init; }

    /// <summary>
    /// The minimum <see cref="System.Version"/> of the dependency module.
    /// </summary>
    /// <remarks>
    /// Versions are resolved by utilizing semantic versioning, i.e. the version of the dependency module
    /// may not be less than the version referenced here. It also must have the same major version.
    /// </remarks>
    [JsonPropertyName("version"), Required]
    public Version Version { get; init; } = new();
}