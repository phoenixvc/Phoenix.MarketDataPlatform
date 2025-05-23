using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Repositories;

public static class VersioningExtensions
{
    /// <summary>
    /// Gets the next available version number for a market data entity with the specified criteria.
    /// </summary>
    /// <typeparam name="T">The type of market data entity</typeparam>
    /// <param name="repo">The repository instance</param>
    /// <param name="dataType">The data type identifier</param>
    /// <param name="assetClass">The asset class</param>
    /// <param name="assetId">The asset identifier</param>
    /// <param name="region">The region code</param>
    /// <param name="asOfDate">The as-of date</param>
    /// <param name="documentType">The document type</param>
    /// <returns>The next version number (starting from 1 for new entities)</returns>
    public static async Task<int> GetNextVersionAsync<T>(
        this CosmosRepository<T> repo,
        string dataType,
        string assetClass,
        string assetId,
        string region,
        DateOnly asOfDate,
        string documentType)
        where T : class, IMarketDataEntity
    {
        var container = repo.GetContainer();
        var query = container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution: false)
            .Where(e =>
                e.AssetId == assetId &&
                e.AssetClass == assetClass &&
                e.Region == region &&
                e.DataType == dataType &&
                e.DocumentType == documentType &&
                e.AsOfDate == asOfDate)
            .OrderByDescending(e => e.Version)
            .Take(1)
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            var latest = response.FirstOrDefault();
            if (latest != null)
            {
                // Handle the nullable Version property properly
                int currentVersion = latest.Version ?? 0;
                return currentVersion + 1;
            }
        }
        return 1;
    }

    /// <summary>
    /// Updates a market data entity with optimistic concurrency control to prevent race conditions
    /// </summary>
    /// <typeparam name="T">The type of market data entity</typeparam>
    /// <param name="repo">The repository instance</param>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    /// <exception cref="CosmosException">Thrown when a concurrency conflict occurs</exception>
    public static async Task<T> UpdateWithOptimisticConcurrencyAsync<T>(
        this CosmosRepository<T> repo,
        T entity)
        where T : class, IMarketDataEntity
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        try
        {
            // Cosmos DB uses ETags for optimistic concurrency control
            // When using the SDK, this happens automatically if you use
            // the AccessCondition with the entity's ETag
            var container = repo.GetContainer();

            // The ETag for the entity should be stored somewhere, 
            // typically as a property of the entity itself
            // For this example, we'll assume the ETag is stored in a custom property
            string? etag = (entity as IETaggable)?.ETag; // Fix: Changed to nullable string type

            ItemRequestOptions? options = null; // Fix: Mark options as nullable
            if (!string.IsNullOrEmpty(etag))
            {
                options = new ItemRequestOptions
                {
                    IfMatchEtag = etag
                };
            }

            // Assuming the entity's ID and partition key can be determined from its properties
            string id = entity.Id;
            string partitionKey = $"{entity.DataType}:{entity.AssetClass}:{entity.Region}";

            var response = await container.ReplaceItemAsync(
                entity,
                id,
                new PartitionKey(partitionKey),
                options);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
        {
            // Handle the concurrency conflict
            // You might want to:
            // 1. Reload the latest version of the entity
            // 2. Apply your changes to the latest version
            // 3. Try the update again

            throw new ConcurrencyException(
                "Another process has modified this entity. Please reload and try again.",
                ex);
        }
    }
}

/// <summary>
/// Interface for entities that support ETag-based concurrency control
/// </summary>
public interface IETaggable
{
    /// <summary>
    /// Gets or sets the ETag value for optimistic concurrency control
    /// </summary>
    string ETag { get; set; }
}

/// <summary>
/// Exception thrown when a concurrency conflict is detected
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
    public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
}