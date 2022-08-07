using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace X39.Hosting.Modularization.Configuration;

/// <summary>
/// Provides build-related information.
/// </summary>
[PublicAPI]
public record BuildInformation
{
    /// <summary>
    /// The version of the module.
    /// </summary>
    [JsonPropertyName("version"), Required]
    public Version Version { get; init; } = new();

    /// <summary>
    /// The creation timestamp of the module.
    /// </summary>
    [JsonPropertyName("release-timestamp"), Required]
    public DateTimeOffset ReleaseTimeStamp { get; init; } = DateTimeOffset.MinValue;

    /// <summary>
    /// A reference to the underlying build system, identifying the specific build for the module.
    /// </summary>
    [JsonPropertyName("vcs-reference"), Required]
    public string? VersionControlReference { get; init; }
}