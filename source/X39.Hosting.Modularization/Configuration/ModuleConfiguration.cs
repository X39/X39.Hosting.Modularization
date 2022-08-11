using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
    /// A sample json configuration file.
    /// </summary>
    public static string JsonSample
    {
        get
        {
            var config = new ModuleConfiguration
            {
                Guid         = Guid.NewGuid(),
                StartDll     = "file.dll",
                Dependencies = new ModuleDependency
                {
                    Guid = Guid.NewGuid(),
                    Version = new Version(1,2,3,4),
                }.MakeArray(),
                Information = new LocalizedInformation
                {
                    Culture = new CultureInfo("en-US"),
                    Description =
                        "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor " +
                        "invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam " +
                        "et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est " +
                        "Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, " +
                        "sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, " +
                        "sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. " +
                        "Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.",
                    Name = "Sample module config",
                    LicenseAgreement = new LicenseInformation
                    {
                        Licensor              = null,
                        LicenseUrl            = "https://www.gnu.org/licenses/lgpl-3.0.txt",
                        SpdxLicenseIdentifier = "LGPL-3.0-only",
                    },
                }.MakeArray(),
                UsedLicenses = new LicenseInformation
                {
                    Licensor              = "Depending Library Name",
                    LicenseUrl            = "https://www.gnu.org/licenses/lgpl-3.0.txt",
                    SpdxLicenseIdentifier = "LGPL-3.0-only",
                }.MakeArray(),
                Build = new BuildInformation
                {
                    Version                 = new Version(1, 0, 0, 0),
                    ReleaseTimeStamp        = DateTimeOffset.Now,
                    VersionControlReference = Guid.NewGuid().ToString(),
                },
                General = new GeneralInformation
                {
                    Company      = "string",
                    Contributors = new[] {"name a", "name b", "name c"},
                    Copyright    = $"Sample Corp © {DateTime.Today.Year}",
                    Homepage     = "https://example.com",
                    Trademark    = "Trademark ™",
                    SourceUrl    = "https://github.com/example/repository",
                    SupportUrl   = "ttps://github.com/example/repository/issues",
                },
            };
            var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                WriteIndented  = true,
                NumberHandling = JsonNumberHandling.Strict,
            };
            using var jsonDocument = JsonSerializer.SerializeToDocument(config, serializerOptions);
            return jsonDocument.RootElement.ToString();
        }
    }

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
    /// The <see cref="ModuleDependency"/>'s of the modules that this module depends on.
    /// </summary>
    /// <remarks>
    /// This defines the order in which modules are loaded.
    /// </remarks>
    [JsonPropertyName("dependencies")]
    public IReadOnlyCollection<ModuleDependency> Dependencies { get; init; } =
        ArraySegment<ModuleDependency>.Empty;

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