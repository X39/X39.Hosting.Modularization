using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace X39.Hosting.Modularization.Configuration;

/// <summary>
/// Offers human-readable information about the module that is do be displayed to the user.
/// </summary>
[PublicAPI]
public record LocalizedInformation
{
    /// <summary>
    /// The language of the information.
    /// </summary>
    /// <example>en-US</example>
    /// <example>de</example>
    /// <example>de-CH</example>
    [JsonPropertyName("language"), Required]
    public CultureInfo Culture { get; init; } = CultureInfo.CurrentCulture;

    /// <summary>
    /// The name of the module.
    /// </summary>
    [JsonPropertyName("name"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// A description of what the module does.
    /// </summary>
    [JsonPropertyName("description"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The license of the module.
    /// </summary>
    [JsonPropertyName("license"), Required]
    public LicenseInformation LicenseAgreement { get; init; } = new();
}