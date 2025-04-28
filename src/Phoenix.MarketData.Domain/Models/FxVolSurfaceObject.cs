using Newtonsoft.Json;
using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Domain.Models
{
    /// <summary>
    /// Represents a Foreign Exchange (FX) volatility surface object, which is a type of market data object.
    /// This class encapsulates data related to FX volatility surfaces, including metadata, identifiers,
    /// and the volatility surface's underlying structure such as strikes, maturities, and associated volatilities.
    /// </summary>
    public class FxVolSurfaceObject : IMarketDataObject
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

        // --- Surface-specific Payload ---
        [JsonProperty("surface")]
        public required VolatilitySurface Surface { get; set; }
    }

    public class VolatilitySurface
    {
        [JsonProperty("strikes")]
        public required List<double> Strikes { get; set; }

        [JsonProperty("maturities")]
        public required List<string> Maturities { get; set; }

        [JsonProperty("volMatrix")]
        public required List<List<double>> Matrix { get; set; }
    }
}