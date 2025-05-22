using System;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace Phoenix.MarketData.Core.Events.IntegrationEvents
{
    /// <summary>
    /// Common interface for all market data integration events
    /// </summary>
    public interface IMarketDataIntegrationEvent
    {
        /// <summary>Gets the unique identifier of the market data</summary>
        string Id { get; }

        /// <summary>Gets the asset identifier</summary>
        string AssetId { get; }

        /// <summary>Gets the asset class</summary>
        string AssetClass { get; }

        /// <summary>Gets the data type</summary>
        string DataType { get; }

        /// <summary>Gets the document type</summary>
        string DocumentType { get; }

        /// <summary>Gets the optional version</summary>
        int? Version { get; }

        /// <summary>Gets the event type identifier</summary>
        string EventType { get; }

        /// <summary>Gets the timestamp when the event occurred</summary>
        DateTimeOffset Timestamp { get; }
    }

    /// <summary>
    /// Abstract base class implementing common functionality for market data integration events
    /// </summary>
    public abstract class MarketDataIntegrationEventBase : IMarketDataIntegrationEvent
    {
        /// <summary>Gets the unique identifier of the market data</summary>
        public required string Id { get; init; }

        /// <summary>Gets the asset identifier</summary>
        public required string AssetId { get; init; }

        /// <summary>Gets the asset class</summary>
        public required string AssetClass { get; init; }

        /// <summary>Gets the data type</summary>
        public required string DataType { get; init; }

        /// <summary>Gets the document type</summary>
        public required string DocumentType { get; init; }

        /// <summary>Gets the optional version</summary>
        public int? Version { get; init; }

        /// <summary>Gets the event type identifier - implemented by derived classes</summary>
        public abstract string EventType { get; }

        /// <summary>Gets the timestamp when the event occurred</summary>
        public required DateTimeOffset Timestamp { get; init; }

        /// <summary>
        /// Default constructor for object initializer syntax
        /// </summary>
        protected MarketDataIntegrationEventBase()
        {
        }

        /// <summary>
        /// Constructor that sets all required members
        /// </summary>
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

    /// <summary>
    /// Event raised when market data is created
    /// </summary>
    public class MarketDataCreatedIntegrationEvent : MarketDataIntegrationEventBase
    {
        /// <summary>Gets the event type identifier</summary>
        public override string EventType => "Created";

        /// <summary>
        /// Default constructor for object initializer
        /// </summary>
        public MarketDataCreatedIntegrationEvent() : base()
        {
        }

        /// <summary>
        /// Constructor that sets all required properties
        /// </summary>
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

    /// <summary>
    /// Event raised when market data is changed
    /// </summary>
    public class MarketDataChangedIntegrationEvent : MarketDataIntegrationEventBase
    {
        /// <summary>Gets the event type identifier</summary>
        public override string EventType => "Changed";

        /// <summary>
        /// Default constructor for object initializer
        /// </summary>
        public MarketDataChangedIntegrationEvent() : base()
        {
        }

        /// <summary>
        /// Constructor that sets all required properties
        /// </summary>
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