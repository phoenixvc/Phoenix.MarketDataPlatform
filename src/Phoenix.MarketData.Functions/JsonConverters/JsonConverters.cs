using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Phoenix.MarketData.Functions.JsonConverters
{
    /// <summary>
    /// Provides custom JSON conversion for DateOnly values
    /// </summary>
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        private const string DateFormat = "yyyy-MM-dd";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle null values
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            var dateString = reader.GetString();

            // Handle empty strings
            if (string.IsNullOrEmpty(dateString))
                return default;

            // Try to parse with proper error handling
            if (DateOnly.TryParse(dateString, out var date))
                return date;

            // Try with specific format
            if (DateOnly.TryParseExact(dateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out date))
                return date;

            throw new JsonException($"Unable to parse \"{dateString}\" as a valid DateOnly value.");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(DateFormat));
    }

    /// <summary>
    /// Provides custom JSON conversion for TimeOnly values
    /// </summary>
    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
    {
        private const string TimeFormat = "HH:mm:ss";

        public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle null values
            if (reader.TokenType == JsonTokenType.Null)
                return default;

            var timeString = reader.GetString();

            // Handle empty strings
            if (string.IsNullOrEmpty(timeString))
                return default;

            // Try to parse with proper error handling
            if (TimeOnly.TryParse(timeString, out var time))
                return time;

            // Try with specific format
            if (TimeOnly.TryParseExact(timeString, TimeFormat, null, System.Globalization.DateTimeStyles.None, out time))
                return time;

            throw new JsonException($"Unable to parse \"{timeString}\" as a valid TimeOnly value.");
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(TimeFormat));
    }
}