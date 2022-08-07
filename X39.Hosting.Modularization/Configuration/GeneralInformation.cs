using System.Text.Json.Serialization;

namespace X39.Hosting.Modularization.Configuration;

/// <summary>
/// Offers general information about the module.
/// </summary>
[PublicAPI]
public class GeneralInformation
{
    /// <summary>
    /// The company name of the vendor.
    /// </summary>
    [JsonPropertyName("company")]
    public string? Company { get; init; }

    /// <summary>
    /// The name of the contributors
    /// </summary>
    [JsonPropertyName("contributors")]
    public IReadOnlyCollection<string>? Contributors { get; init; }
            
    /// <summary>
    /// The url at which support can be obtained.
    /// </summary>
    [JsonPropertyName("support-url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SupportUrl { get; init; }
            
    /// <summary>
    /// The url at which the source code can be obtained.
    /// </summary>
    [JsonPropertyName("source-url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SourceUrl { get; init; }
            
    /// <summary>
    /// The project url.
    /// </summary>
    [JsonPropertyName("homepage"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Homepage { get; init; }

    /// <summary>
    /// The trademark of the software if any.
    /// </summary>
    [JsonPropertyName("trademark"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Trademark { get; init; }

    /// <summary>
    /// Copyright information if applicable.
    /// </summary>
    /// <example>
    /// Copyright (c) 2020 E-Vil Corp. All rights reserved.
    /// </example>
    [JsonPropertyName("copyright"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Copyright { get; init; }
}