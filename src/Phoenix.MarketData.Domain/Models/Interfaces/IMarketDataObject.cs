namespace Phoenix.MarketData.Domain.Models.Interfaces
{
    /// <summary>
    /// Represents a market data object with key properties to track asset and versioning information.
    /// </summary>
    public interface IMarketDataObject
    {
        /// <summary>
        /// The Id of the data in the format, [dataType].[assetclass]__[asset]__[date]__[documentType]__[version], e.g.,
        /// price.fx__BTCUSD__20250427__official__1. This Id is determined by the method and is not to be set.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The Schema Version indicates the version of the schema used to describe the structure
        /// and metadata of the object. It provides a mechanism to manage changes or updates
        /// to the schema and ensures compatibility with data processing systems.
        /// </summary>
        string SchemaVersion { get; set; }

        string? Version { get; set; }
        
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
        DateTimeOffset CreatedTimestamp { get; }

        /// <summary>
        /// Represents the date to which the market data applies. Typically used to indicate
        /// the effective date of the data, ensuring it corresponds to the appropriate time
        /// frame for analysis or usage.
        /// </summary>
        DateOnly AsOfDate { get; set; }
    }
}