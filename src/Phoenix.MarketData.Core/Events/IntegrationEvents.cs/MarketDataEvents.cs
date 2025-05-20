using System.Text.Json.Serialization;

namespace Phoenix.MarketData.Core.Events.IntegrationEvents
{
    public class MarketDataChangedIntegrationEvent
    {
        public required string Id { get; init; }
        public required string AssetId { get; init; }
        public required string AssetClass { get; init; }
        public required string DataType { get; init; }
        public required string DocumentType { get; init; }
        public int? Version { get; init; }
        public string EventType { get; init; } = "Changed";
        public required DateTimeOffset Timestamp { get; init; }

        [JsonConstructor]
        public MarketDataChangedIntegrationEvent(
            string id,
            string assetId,
            string assetClass,
            string dataType,
            string documentType,
            int? version,
            DateTimeOffset timestamp)
        {
            Id = id;
            AssetId = assetId;
            AssetClass = assetClass;
            DataType = dataType;
            DocumentType = documentType;
            Version = version;
            Timestamp = timestamp;
        }
    }

    public class MarketDataCreatedIntegrationEvent
    {
        public string Id { get; }
        public string AssetId { get; }
        public string AssetClass { get; }
        public string DataType { get; }
        public string DocumentType { get; }
        public int? Version { get; }
        public string EventType { get; } = "Created";
        public DateTimeOffset Timestamp { get; }

        [JsonConstructor]
        public MarketDataCreatedIntegrationEvent(
            string id,
            string assetId,
            string assetClass,
            string dataType,
            string documentType,
            int? version,
            DateTimeOffset timestamp)
        {
            Id = id;
            AssetId = assetId;
            AssetClass = assetClass;
            DataType = dataType;
            DocumentType = documentType;
            Version = version;
            Timestamp = timestamp;
        }
    }
}
