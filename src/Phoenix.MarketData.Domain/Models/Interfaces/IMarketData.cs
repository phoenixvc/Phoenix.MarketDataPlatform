namespace Phoenix.MarketData.Domain.Models.Interfaces
{
    /// <summary>
    /// Represents a market data object with key properties to track asset and versioning information.
    /// </summary>
    public interface IMarketData
    {
        /// <summary>
        /// The Id of the data in the format, [dataType].[assetclass]__[asset]__[region]__[date]__[documentType]__[version], e.g.,
        /// price.spot__fx__BTCUSD__global__20250427__official__1.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The Schema Version indicates the version of the schema used to describe the structure
        /// and metadata of the object. It provides a mechanism to manage changes or updates
        /// to the schema and ensures compatibility with data processing systems.
        /// </summary>
        string SchemaVersion { get; set; }

        /// <summary>
        /// Represents the version of a market data object. This property is used to track
        /// incremental updates or changes to the data over time.
        /// </summary>
        int? Version { get; set; }

        /// <summary>
        /// The unique identifier of the asset, used as the partition key. For example, USDZAR, TSLA (NASDAQ), etc.
        /// </summary>
        string AssetId { get; set; }
        
        /// <summary>
        /// The asset class, i.e., FX, Equity, Rates, Credit, [Hybrid versions]
        /// </summary>
        string AssetClass { get; set; }

        /// <summary>
        /// Specifies the type of data being represented, used to categorize and differentiate
        /// among various types of market data such as spotprice, forwardprice, curve, or volsurface
        /// other financial data sets. 
        /// </summary>
        string DataType { get; set; }

        /// <summary>
        /// Specifies the region applicable for the market data. This property identifies the geographical or
        /// jurisdictional location relevant to the asset or dataset. The value is expected to be set based on the
        /// context of the market data being represented.
        /// </summary>
        string Region { get; set; }

        /// <summary>
        /// A collection of tags associated with the data object, used to provide additional
        /// metadata or categorization for filtering, searching, or context purposes.
        /// </summary>
        List<string> Tags { get; set; }

        /// <summary>
        /// Specifies the type or category of the document, such as "official", "intraday", or "live",
        /// used to indicate the nature or version of the market data document.
        /// </summary>
        string DocumentType { get; set; }

        /// <summary>
        /// The timestamp indicating the date, time and timezone when the data was generated or last updated,
        /// in ISO 8601 format, e.g., "2023-10-19T15:23:00Z".
        /// </summary>
        DateTimeOffset CreateTimestamp { get; }

        /// <summary>
        /// Represents the date to which the market data applies. Typically used to indicate
        /// the effective date of the data, ensuring it corresponds to the appropriate time
        /// frame for analysis or usage.
        /// </summary>
        DateOnly AsOfDate { get; set; }

        /// <summary>
        /// The specific time of day corresponding to when the market data is relevant.
        /// This property allows for a more granular representation of data applicability
        /// in conjunction with the associated AsOfDate.
        /// </summary>
        TimeOnly? AsOfTime { get; set; }
    }
}