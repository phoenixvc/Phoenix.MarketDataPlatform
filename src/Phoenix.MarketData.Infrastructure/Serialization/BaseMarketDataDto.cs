using System.Text.Json.Serialization;
using Phoenix.MarketData.Infrastructure.Serialization.JsonConverters;

namespace Phoenix.MarketData.Infrastructure.Serialization;

public class BaseMarketDataDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public int? Version { get; set; }

    [JsonPropertyName("assetId")]
    public string AssetId { get; set; } = string.Empty;

    [JsonPropertyName("assetClass")]
    public string AssetClass { get; set; } = string.Empty;

    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = string.Empty;

    [JsonPropertyName("region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("documentType")]
    public string DocumentType { get; set; } = string.Empty;

    [JsonPropertyName("createTimestamp")]
    public DateTimeOffset? CreateTimestamp { get; set; }

    [JsonPropertyName("asOfDate")]
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateOnly AsOfDate { get; set; }

    [JsonPropertyName("asOfTime")]
    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly? AsOfTime { get; set; }

    [JsonConstructor]
    public BaseMarketDataDto(
        string id,
        string schemaVersion,
        int? version,
        string assetId,
        string assetClass,
        string dataType,
        string region,
        string documentType,
        DateTimeOffset createTimeStamp,
        DateOnly asOfDate,
        TimeOnly? asOfTime,
        List<string>? tags)
    {
        Id = id;
        SchemaVersion = schemaVersion;
        Version = version;
        AssetId = assetId;
        AssetClass = assetClass;
        DataType = dataType;
        Region = region;
        DocumentType = documentType;
        CreateTimestamp = createTimeStamp;
        AsOfDate = asOfDate;
        AsOfTime = asOfTime;
        Tags = tags ?? [];
    }
}
