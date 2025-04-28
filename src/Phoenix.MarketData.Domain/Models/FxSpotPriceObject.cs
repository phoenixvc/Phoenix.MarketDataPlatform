using Newtonsoft.Json;
using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Domain.Models
{
    /// <summary>
    /// Represents the spot price object for foreign exchange (FX) data. This class provides
    /// properties that describe the details of a specific FX spot price, including
    /// identification, versioning, metadata, and price information.
    /// </summary>
    public class FxSpotPriceObject : IMarketDataObject
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("schemaVersion")]
        public required string SchemaVersion { get; set; }

        [JsonProperty("version")]
        public required string Version { get; set; }

        [JsonProperty("assetId")]
        public required string AssetId { get; set; }

        [JsonProperty("assetClass")]
        public required string AssetClass { get; set; }

        [JsonProperty("dataType")]
        public required string DataType { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonProperty("documentType")]
        public required string DocumentType { get; set; }

        [JsonProperty("timestamp")]
        public required DateTime Timestamp { get; set; }

        // --- Spot-specific Payload ---
        [JsonProperty("price")]
        public required double Price { get; set; }
    }
}