using System.ComponentModel;
using Newtonsoft.Json;

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

    [JsonProperty("version")]
    public string? Version { get; set; }

    [JsonProperty("assetId")]
    public string AssetId { get; set; }

    [JsonProperty("assetClass")]
    public string AssetClass { get; set; }

    [JsonProperty("dataType")]
    public string DataType { get; set; }

    [JsonProperty("region")]
    public string Region { get; set; }

    [JsonProperty("tags")]
    public List<string> Tags { get; set; }

    [JsonProperty("documentType")]
    public string DocumentType { get; set; }

    [JsonProperty("createTimestamp")]
    public DateTimeOffset CreateTimestamp { get; set; }

    [JsonProperty("asOfDate")]
    [JsonConverter(typeof(DateOnlyConverter))]
    public DateOnly AsOfDate { get; set; }
    
    [JsonProperty("asOfTime")]
    [JsonConverter(typeof(TimeOnlyConverter))]
    public TimeOnly? AsOfTime { get; set; }
    
    public void CopyBasePropertiesFrom(BaseMarketDataDto from)
    {
        Id = from.Id;
        SchemaVersion = from.SchemaVersion;
        Version = from.Version;
        AssetId = from.AssetId;
        AssetClass = from.AssetClass;
        DataType = from.DataType;
        Region = from.Region;
        DocumentType = from.DocumentType;
        CreateTimestamp = from.CreateTimestamp;
        AsOfDate = from.AsOfDate;
        AsOfTime = from.AsOfTime;
        Tags = from.Tags;
    }
}