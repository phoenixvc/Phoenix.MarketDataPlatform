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
        private string? _id; // Backing field for Id
        private DateTimeOffset? _createTimeStamp; // Backing field for Id
        private string? _version;
        
        [JsonProperty("id")] // Ensures this property is deserialized from JSON
        public string Id
        {
            get => _id ??= CalculateId(); // If not set, calculate it
            private set => _id = value;  // Can only be set during deserialization
        }
        
        [JsonProperty("schemaVersion")]
        public required string SchemaVersion { get; set; }
        
        [JsonProperty("version")]
        public string? Version
        {
            get => _version;
            set
            {
                _version = value;
                
                // Invalidate the ID if the Version changes
                _id = CalculateId();
            }
        }

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

        [JsonProperty("createTimestamp")]
        public DateTimeOffset CreatedTimestamp
        {
            get => _createTimeStamp ??= DateTimeOffset.UtcNow;
            private set => _createTimeStamp = value;
        }

        [JsonProperty("asOfDate")]
        public required DateOnly AsOfDate { get; set; }

        private string CalculateId()
        {
            var id = string.Join("__", new [] {
                DataType, AssetClass, AssetId, AsOfDate.ToString("yyyyMMdd"), DocumentType});
            if (Version != null)
                id += $"__{Version}";
            
            return id;
        }
        
        // --- Spot-specific Payload ---
        [JsonProperty("price")]
        public required double Price { get; set; }
    }
}