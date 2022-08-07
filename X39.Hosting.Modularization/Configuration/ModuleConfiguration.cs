using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace X39.Hosting.Modularization.Configuration;

/// <summary>
/// Represents the structure of the module configuration file.
/// </summary>
[PublicAPI]
public record ModuleConfiguration
{
    /// <summary>
    /// The name of the module configuration file.
    /// </summary>
    [PublicAPI] public const string FileName = "module.json";

    /// <summary>
    /// Reads the module configuration from the specified <paramref name="moduleDirectory"/> and returns
    /// a <see cref="ModuleConfiguration"/> if it exists.
    /// </summary>
    /// <remarks>
    /// The module configuration file is expected to be named according to <see cref="FileName"/>.
    /// </remarks>
    /// <param name="moduleDirectory">The directory to load the module configuration from.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>A <see cref="ModuleConfiguration"/> if it exists.</returns>
    public static async Task<ModuleConfiguration?> TryLoadAsync(
        string moduleDirectory,
        CancellationToken cancellationToken)
    {
        var configPath = Path.Combine(moduleDirectory, FileName);
        if (!File.Exists(configPath))
            return null;
        await using var fileStream = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await JsonSerializer.DeserializeAsync<ModuleConfiguration>(
            fileStream,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// The <see cref="Guid"/> that uniquely identifies the module.
    /// </summary>
    [JsonPropertyName("uuid"), Required]
    public Guid Guid { get; init; }

    /// <summary>
    /// The library containing the module.
    /// </summary>
    [JsonPropertyName("start-dll"), Required]
    public string StartDll { get; init; } = string.Empty;

    /// <summary>
    /// Offers localized, human readable names for the module.
    /// </summary>
    [JsonPropertyName("info"), Required]
    public IReadOnlyCollection<LocalizedInformation> Information { get; init; } =
        ArraySegment<LocalizedInformation>.Empty;

    /// <summary>
    /// The <see cref="Guid"/>'s of the modules that this module depends on.
    /// </summary>
    /// <remarks>
    /// This defines the order in which modules are loaded.
    /// </remarks>
    [JsonPropertyName("dependencies")]
    public IReadOnlyCollection<Guid> Dependencies { get; init; } =
        ArraySegment<Guid>.Empty;

    /// <summary>
    /// Provides <see cref="GeneralInformation"/> for the module.
    /// </summary>
    [JsonPropertyName("general"), Required]
    public GeneralInformation General { get; init; } = new();

    /// <summary>
    /// Provides <see cref="BuildInformation"/> for the module.
    /// </summary>
    [JsonPropertyName("build"), Required]
    public BuildInformation Build { get; init; } = new();

    /// <summary>
    /// The licenses used by the module.
    /// </summary>
    [JsonPropertyName("used-licenses")]
    public IReadOnlyCollection<LicenseInformation> UsedLicenses { get; init; } = ArraySegment<LicenseInformation>.Empty;
}