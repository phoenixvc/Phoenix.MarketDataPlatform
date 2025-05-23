using System;
using System.Collections.Generic;

namespace Phoenix.MarketData.Domain.Models
{
    /// <summary>
    /// Base entity interface with identifier
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets the unique identifier for this entity
        /// </summary>
        string Id { get; }
    }

    /// <summary>
    /// Common base interface with properties shared across all market data entities
    /// </summary>
    public interface IMarketDataEntityBase : IEntity
    {
        /// <summary>
        /// Gets or sets the schema version of this market data entity
        /// </summary>
        string SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the asset identifier
        /// </summary>
        string AssetId { get; set; }

        /// <summary>
        /// Gets or sets the asset class (equity, fixed_income, etc.)
        /// </summary>
        string AssetClass { get; set; }

        /// <summary>
        /// Gets or sets the data type (price, yield, etc.)
        /// </summary>
        string DataType { get; set; }

        /// <summary>
        /// Gets or sets the geographical region
        /// </summary>
        string Region { get; set; }

        /// <summary>
        /// Gets the collection of tags associated with this entity
        /// </summary>
        IReadOnlyList<string> Tags { get; }

        /// <summary>
        /// Gets or sets the document type
        /// </summary>
        string DocumentType { get; set; }

        /// <summary>
        /// Gets the timestamp when this entity was created
        /// </summary>
        DateTimeOffset CreateTimestamp { get; }

        /// <summary>
        /// Gets or sets the business date for this market data
        /// </summary>
        DateOnly AsOfDate { get; set; }

        /// <summary>
        /// Gets or sets the optional time component for this market data
        /// </summary>
        TimeOnly? AsOfTime { get; set; }
    }

    /// <summary>
    /// Standard market data entity with optional versioning
    /// </summary>
    public interface IMarketDataEntity : IMarketDataEntityBase
    {
        /// <summary>
        /// Gets or sets the optional version of this market data entity
        /// </summary>
        int? Version { get; set; }
    }
}