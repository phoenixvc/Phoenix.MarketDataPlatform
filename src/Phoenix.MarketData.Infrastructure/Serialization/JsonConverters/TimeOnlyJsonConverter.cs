using System.Globalization;
using Newtonsoft.Json;

namespace Phoenix.MarketData.Infrastructure.Serialization.JsonConverters;

/// <summary>
/// Converts a TimeOnly value to and from JSON using a specified time format ("HH:mm:ss").
/// </summary>
/// <remarks>
/// This converter allows the serialization and deserialization of the <see cref="TimeOnly"/> type
/// in JSON representation, ensuring that time values are consistently handled in a specific
/// string format. The format used is "HH:mm:ss".
/// </remarks>
/// <example>
/// This converter is typically used in scenarios where time information needs to be
/// serialized or deserialized in JSON payloads. It is particularly useful for APIs or other
/// data interactions that require time values to be represented as strings in a uniform format.
/// </example>
public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private const string Format = "HH:mm:ss"; // Example: "14:30:00"

    public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer)
    {
        // Serialize TimeOnly to a string that represents the time
        writer.WriteValue(value.ToString(Format));
    }

    public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // Deserialize the string back to TimeOnly
        var str = reader.Value?.ToString();
        return TimeOnly.ParseExact(str!, Format, CultureInfo.InvariantCulture);
    }
}