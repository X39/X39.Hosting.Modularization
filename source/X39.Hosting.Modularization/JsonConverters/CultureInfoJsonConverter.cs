using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace X39.Hosting.Modularization.JsonConverters;

/// <summary>
/// Offers conversion of <see cref="CultureInfo"/> to and from JSON for System.Text.Json.
/// </summary>
public class CultureInfoJsonConverter : JsonConverter<CultureInfo>
{
    /// <inheritdoc />
    public override CultureInfo? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        return str.IsNullOrWhiteSpace() ? null : new CultureInfo(str);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}