using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace Phoenix.MarketData.Infrastructure.Serialization.JsonConverters;

/// <summary>
/// Converts a TimeOnly value to and from JSON using the "HH:mm:ss" format.
/// </summary>
public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string Format = "HH:mm:ss";

    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Get the string from JSON
        var str = reader.GetString();

        if (string.IsNullOrEmpty(str))
            throw new JsonException("Cannot convert null or empty string to TimeOnly.");

        if (TimeOnly.TryParseExact(str, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            return result;

        throw new JsonException($"Cannot convert '{str}' to TimeOnly (expected format: {Format}).");
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        // Serialize TimeOnly as "HH:mm:ss"
        writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }
}
