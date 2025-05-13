using System.Globalization;
using Newtonsoft.Json;

namespace Phoenix.MarketData.Infrastructure.Serialization.JsonConverters
{
    /// <summary>
    /// A custom JSON converter for serializing and deserializing <see cref="DateOnly"/> values.
    /// </summary>
    /// <remarks>
    /// This converter handles <see cref="DateOnly"/> types by formatting them as strings
    /// in the "yyyy-MM-dd" format during serialization and deserialization.
    /// </remarks>
    /// <example>
    /// Use this converter with <see cref="JsonConverterAttribute"/> to convert <see cref="DateOnly"/> properties in JSON.
    /// </example>
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";

        public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString(Format, CultureInfo.InvariantCulture));
        }

        public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var str = reader.Value?.ToString();
            return DateOnly.ParseExact(str!, Format, CultureInfo.InvariantCulture);
        }
    }
}