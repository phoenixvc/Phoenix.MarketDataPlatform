using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Core.Repositories
{
    /// <summary>
    /// Repository interface for market data entities that provides CRUD operations with versioning support.
    /// </summary>
    /// <typeparam name="T">The type of market data entity this repository manages</typeparam>
    public interface IRepository<T> where T : IMarketDataEntity
    {
        /// <summary>
        /// Retrieves a market data entity by its specific version.
        /// </summary>
        /// <returns>A tuple containing the entity (if found) and its ETag for concurrency control</returns>
        Task<(T? Result, string? ETag)> GetBySpecifiedVersionAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType,
            int version);

        /// <summary>
        /// Retrieves the latest version of a market data entity.
        /// </summary>
        /// <returns>A tuple containing the entity (if found) and its ETag for concurrency control</returns>
        Task<(T? Result, string? ETag)> GetByLatestVersionAsync(
            string dataType,
            string assetClass,
            string assetId,
            string region,
            DateOnly asOfDate,
            string documentType);

        /// <summary>
        /// Saves a market data entity to the repository, creating a new version if it already exists.
        /// </summary>
        /// <returns>The ID of the saved entity</returns>
        Task<string> SaveAsync(T marketData);

        /// <summary>
        /// Queries market data entities with optional filtering.
        /// </summary>
        /// <param name="assetId">Optional filter by asset ID; if null, returns all assets</param>
        /// <param name="fromDate">Optional filter for minimum date; if null, returns from the earliest date</param>
        /// <param name="toDate">Optional filter for maximum date; if null, returns up to the latest date</param>
        /// <returns>A collection of market data entities matching the filters</returns>
        Task<IEnumerable<T>> QueryAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);
    }
}