using Newtonsoft.Json;
using Phoenix.MarketData.Infrastructure.Serialization.JsonConverters;

namespace Phoenix.MarketData.Infrastructure.Serialization;

/// <summary>
/// Represents the base implementation of a market data object, providing core properties
/// and functionality for identifying and managing market data records.
/// </summary>
public class BaseMarketDataDto 
{
    [JsonProperty("id")] // Ensures this property is deserialized from JSON
    public string Id { get; set; }

    [JsonProperty("schemaVersion")]
    public string SchemaVersion { get; set; }

    [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
    public int? Version { get; set; }

    [JsonProperty("assetId")]
    public string AssetId { get; set; }

    [JsonProperty("assetClass")]
    public string AssetClass { get; set; }

    [JsonProperty("dataType")]
    public string DataType { get; set; }

    [JsonProperty("region")]
    public string Region { get; set; }

    [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Tags { get; set; }

    [JsonProperty("documentType")]
    public string DocumentType { get; set; }

    [JsonProperty("createTimestamp", NullValueHandling = NullValueHandling.Ignore)]
    public DateTimeOffset? CreateTimestamp { get; set; }

    [JsonProperty("asOfDate")]
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateOnly AsOfDate { get; set; }
    
    [JsonProperty("asOfTime")]
    [JsonConverter(typeof(TimeOnlyJsonConverter))]
    public TimeOnly? AsOfTime { get; set; }

    public BaseMarketDataDto()
    {
    }
    
    public BaseMarketDataDto(string id, string schemaVersion, int? version, string assetId, string assetClass, 
        string dataType, string region, string documentType, DateTimeOffset createTimeStamp, DateOnly asOfDate,
        TimeOnly? asOfTime, List<string> tags)
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