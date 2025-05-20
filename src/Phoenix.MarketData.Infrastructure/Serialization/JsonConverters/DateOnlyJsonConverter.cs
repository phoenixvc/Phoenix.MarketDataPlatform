using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phoenix.MarketData.Infrastructure.Serialization.JsonConverters
{
    /// <summary>
    /// A custom JSON converter for serializing and deserializing <see cref="DateOnly"/> values.
    /// Formats as "yyyy-MM-dd".
    /// </summary>
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string Format = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (string.IsNullOrEmpty(str))
                throw new JsonException("Cannot parse null or empty value as DateOnly.");

            return DateOnly.ParseExact(str, Format, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
}
