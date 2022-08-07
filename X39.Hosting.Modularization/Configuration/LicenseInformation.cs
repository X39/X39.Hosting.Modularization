using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace X39.Hosting.Modularization.Configuration;

/// <summary>
/// Represents a license.
/// The license is not required to be a valid SPDX license,
/// but it is required to have either a SPDX code or a URL where the license can be found.
/// </summary>
[PublicAPI]
public record LicenseInformation
{
    /// <summary>
    /// The SPDX identifier of the license or null if this does not apply.
    /// See <a href="https://spdx.org/licenses/">the official SPDX license list</a> for a full explanation.
    /// </summary>
    [JsonPropertyName("spdx-id"), Required]
    public string? SpdxLicenseIdentifier { get; init; } = null;
            
    /// <summary>
    /// The url of the license or null if this does not apply.
    /// </summary>
    [JsonPropertyName("url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? LicenseUrl { get; init; }
}