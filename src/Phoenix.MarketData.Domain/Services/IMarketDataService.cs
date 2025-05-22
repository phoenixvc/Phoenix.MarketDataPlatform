using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Domain.Services
{
    /// <summary>
    /// Service interface for managing market data operations.
    /// </summary>
    public interface IMarketDataService
    {
        /// <summary>
        /// Publishes new market data to the system.
        /// </summary>
        /// <typeparam name="T">Type of market data entity</typeparam>
        /// <param name="marketData">The market data to publish</param>
        /// <returns>The ID of the published market data</returns>
        Task<string> PublishMarketDataAsync<T>(T marketData) where T : IMarketDataEntity;

        /// <summary>
        /// Updates existing market data in the system.
        /// </summary>
        /// <typeparam name="T">Type of market data entity</typeparam>
        /// <param name="marketData">The updated market data</param>
        /// <returns>True if the update was successful, false otherwise</returns>
        Task<bool> UpdateMarketDataAsync<T>(T marketData) where T : IMarketDataEntity;

        /// <summary>
        /// Deletes market data by its ID.
        /// </summary>
        /// <typeparam name="T">Type of market data entity</typeparam>
        /// <param name="id">The ID of the market data to delete</param>
        /// <returns>True if the deletion was successful, false otherwise</returns>
        Task<bool> DeleteMarketDataAsync<T>(string id) where T : IMarketDataEntity;

        /// <summary>
        /// Gets the latest market data for the specified criteria.
        /// </summary>
        /// <typeparam name="T">Type of market data entity</typeparam>
        /// <param name="dataType">Type of data</param>
        /// <param name="assetClass">Asset class</param>
        /// <param name="assetId">Asset identifier</param>
        /// <param name="region">Region</param>
        /// <param name="asOfDate">As-of date</param>
        /// <param name="documentType">Document type</param>
        /// <returns>The latest market data matching the criteria, or null if not found</returns>
        Task<T?> GetLatestMarketDataAsync<T>(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType) where T : IMarketDataEntity;

        /// <summary>
        /// Queries market data based on filter criteria.
        /// </summary>
        /// <typeparam name="T">Type of market data entity</typeparam>
        /// <param name="filter">Filter criteria for the query</param>
        /// <returns>Collection of market data matching the filter criteria</returns>
        Task<IEnumerable<T>> QueryMarketDataAsync<T>(
            MarketDataQueryFilter filter) where T : IMarketDataEntity;
    }

    /// <summary>
    /// Filter object for market data queries.
    /// </summary>
    public class MarketDataQueryFilter
    {
        /// <summary>
        /// Gets or sets the type of data to query.
        /// </summary>
        public required string DataType { get; set; }

        /// <summary>
        /// Gets or sets the asset class to query.
        /// </summary>
        public required string AssetClass { get; set; }

        /// <summary>
        /// Gets or sets the optional asset identifier to query.
        /// </summary>
        public string? AssetId { get; set; }

        /// <summary>
        /// Gets or sets the optional region to query.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Gets or sets the optional document type to query.
        /// </summary>
        public string? DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the optional start date for the query range.
        /// </summary>
        public DateOnly? FromDate { get; set; }

        /// <summary>
        /// Gets or sets the optional end date for the query range.
        /// </summary>
        public DateOnly? ToDate { get; set; }

        /// <summary>
        /// Gets or sets optional tags to filter by.
        /// </summary>
        public IList<string>? Tags { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of results to return.
        /// </summary>
        public int? MaxResults { get; set; }

        /// <summary>
        /// Gets or sets whether to include only the latest version for each asset.
        /// </summary>
        public bool LatestVersionOnly { get; set; } = true;
    }
}