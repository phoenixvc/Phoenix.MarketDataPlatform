using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis; // Add this for SetsRequiredMembers attribute

namespace Phoenix.MarketData.Core.Events.IntegrationEvents
{
    // Common interface for all market data events
    public interface IMarketDataIntegrationEvent
    {
        string Id { get; }
        string AssetId { get; }
        string AssetClass { get; }
        string DataType { get; }
        string DocumentType { get; }
        int? Version { get; }
        string EventType { get; }
        DateTimeOffset Timestamp { get; }
    }

    // Abstract base class containing all common functionality
    public abstract class MarketDataIntegrationEventBase : IMarketDataIntegrationEvent
    {
        public required string Id { get; init; }
        public required string AssetId { get; init; }
        public required string AssetClass { get; init; }
        public required string DataType { get; init; }
        public required string DocumentType { get; init; }
        public int? Version { get; init; }
        public abstract string EventType { get; }  // Abstract property that derived classes must implement
        public required DateTimeOffset Timestamp { get; init; }

        // Default constructor for object initializer syntax
        protected MarketDataIntegrationEventBase()
        {
        }

        // Constructor that can be called by derived classes
        // This attribute marks the constructor as setting all required members
        [SetsRequiredMembers]
        protected MarketDataIntegrationEventBase(
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

    // Concrete implementation for "Created" events
    public class MarketDataCreatedIntegrationEvent : MarketDataIntegrationEventBase
    {
        // Override with specific event type
        public override string EventType => "Created";

        // Default constructor for object initializer
        public MarketDataCreatedIntegrationEvent() : base()
        {
        }

        // Constructor that sets all required properties
        [JsonConstructor]
        [SetsRequiredMembers]
        public MarketDataCreatedIntegrationEvent(
            string id,
            string assetId,
            string assetClass,
            string dataType,
            string documentType,
            int? version,
            DateTimeOffset timestamp)
            : base(id, assetId, assetClass, dataType, documentType, version, timestamp)
        {
        }
    }

    // Concrete implementation for "Changed" events
    public class MarketDataChangedIntegrationEvent : MarketDataIntegrationEventBase
    {
        // Override with specific event type
        public override string EventType => "Changed";

        // Default constructor for object initializer
        public MarketDataChangedIntegrationEvent() : base()
        {
        }

        // Constructor that sets all required properties
        [JsonConstructor]
        [SetsRequiredMembers]
        public MarketDataChangedIntegrationEvent(
            string id,
            string assetId,
            string assetClass,
            string dataType,
            string documentType,
            int? version,
            DateTimeOffset timestamp)
            : base(id, assetId, assetClass, dataType, documentType, version, timestamp)
        {
        }
    }
}
