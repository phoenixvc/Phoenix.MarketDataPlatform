// src/Phoenix.MarketData.Infrastructure/Repositories/CosmosRepositoryAdditionalMethods.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Repositories
{
    // Additional required methods to implement IRepository<T>
    public partial class CosmosRepository<T> where T : class, IMarketDataEntity
    {
        public async Task<(IEnumerable<T> Items, string? ContinuationToken)> GetPagedAsync(
            int pageSize,
            string? continuationToken = null,
            bool includeSoftDeleted = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // For consistent paging, we need deterministic ordering and filters in the SQL
                string query = "SELECT * FROM c";

                // Apply soft delete filter directly in the SQL query if applicable
                if (!includeSoftDeleted && typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
                {
                    query += " WHERE NOT IS_DEFINED(c.isDeleted) OR c.isDeleted = false";
                }

                // Add a deterministic sort order to ensure reliable paging
                query += " ORDER BY c._ts DESC";

                var queryOptions = new QueryRequestOptions
                {
                    MaxItemCount = pageSize
                };

                // Execute the query with the continuation token
                var queryDefinition = new QueryDefinition(query);
                var feedIterator = _container.GetItemQueryIterator<T>(
                    queryDefinition,
                    continuationToken,
                    queryOptions);

                var results = new List<T>();
                string? nextContinuationToken = null;

                if (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync(cancellationToken);
                    results.AddRange(response);

                    // Get continuation token for the next page
                    nextContinuationToken = response.ContinuationToken;
                }

                _logger.LogDebug("Retrieved page of {Count} items with continuation token: {HasToken}",
                    results.Count, nextContinuationToken != null);

                return (results, nextContinuationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            var entityList = entities.ToList();
            var tasks = new List<Task>();
            var exceptions = new List<Exception>();
            int count = 0;

            foreach (var entity in entityList)
            {
                var task = Task.Run(async () =>
                {
                    try
                    {
                        var partitionKey = GetPartitionKey(entity);
                        await _container.CreateItemAsync(entity, partitionKey, cancellationToken: cancellationToken);
                        Interlocked.Increment(ref count);
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                        _logger.LogError(ex, "Bulk insert failed for entity {EntityType} with ID {Id}: {ErrorMessage}",
                            typeof(T).Name, entity.Id, ex.Message);
                    }
                }, cancellationToken);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            if (exceptions.Count > 0)
                throw new AggregateException($"Bulk insert completed with {exceptions.Count} failures.", exceptions);
            return count;
        }

        public async Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType,
            CancellationToken cancellationToken = default)
        {
            // Check if T implements IMarketDataEntity
            if (!typeof(IMarketDataEntity).IsAssignableFrom(typeof(T)))
            {
                throw new InvalidOperationException(
                    $"Type {typeof(T).Name} must implement IMarketDataEntity to use GetLatestMarketDataAsync");
            }

            return await ExecuteSingleQueryAsync<T>(
                async (ct) =>
                {
                    // Use the assetId for the partition key if possible
                    var queryOptions = new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(assetId)
                    };

                    // Use SQL query since we can't use dynamic in LINQ
                    var queryDefinition = new QueryDefinition(@"
                        SELECT TOP 1 * FROM c 
                        WHERE c.dataType = @dataType 
                        AND c.assetClass = @assetClass 
                        AND c.assetId = @assetId 
                        AND c.region = @region 
                        AND c.asOfDate = @asOfDate 
                        AND c.documentType = @documentType
                        ORDER BY c.version DESC")
                        .WithParameter("@dataType", dataType)
                        .WithParameter("@assetClass", assetClass)
                        .WithParameter("@assetId", assetId)
                        .WithParameter("@region", region)
                        .WithParameter("@asOfDate", asOfDate.ToString("yyyy-MM-dd"))
                        .WithParameter("@documentType", documentType);

                    var iterator = _container.GetItemQueryIterator<T>(
                        queryDefinition,
                        requestOptions: queryOptions);

                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync(ct);
                        return response.FirstOrDefault();
                    }
                    return null;
                },
                "GetLatestMarketData",
                null,
                cancellationToken
            );
        }

        public async Task<int> GetNextVersionAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType,
            CancellationToken cancellationToken = default)
        {
            var latest = await GetLatestMarketDataAsync(
                dataType, assetClass, assetId, region, asOfDate, documentType, cancellationToken);
            return latest is IMarketDataEntity v ? (v.Version ?? 0) + 1 : 1;
        }

        public async Task<IEnumerable<T>> QueryByRangeAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync<T>(
                async (ct) =>
                {
                    // Set default date bounds if not provided to avoid full container scans
                    var effectiveFromDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                    var effectiveToDate = toDate ?? DateTime.UtcNow;

                    // Build SQL query to avoid dynamic in expression trees
                    var queryBuilder = new StringBuilder();
                    queryBuilder.Append(@"
                        SELECT * FROM c 
                        WHERE c.dataType = @dataType 
                        AND c.assetClass = @assetClass ");

                    if (!string.IsNullOrEmpty(assetId))
                    {
                        queryBuilder.Append("AND c.assetId = @assetId ");
                    }

                    queryBuilder.Append(@"
                        AND c.asOfDate >= @fromDate 
                        AND c.asOfDate <= @toDate ");

                    // Add soft delete filter if applicable
                    if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
                    {
                        queryBuilder.Append("AND (NOT IS_DEFINED(c.isDeleted) OR c.isDeleted = false) ");
                    }

                    // Add deterministic ordering
                    queryBuilder.Append("ORDER BY c.asOfDate DESC");

                    var queryDefinition = new QueryDefinition(queryBuilder.ToString())
                        .WithParameter("@dataType", dataType)
                        .WithParameter("@assetClass", assetClass)
                        .WithParameter("@fromDate", DateOnly.FromDateTime(effectiveFromDate).ToString("yyyy-MM-dd"))
                        .WithParameter("@toDate", DateOnly.FromDateTime(effectiveToDate).ToString("yyyy-MM-dd"));

                    if (!string.IsNullOrEmpty(assetId))
                    {
                        queryDefinition = queryDefinition.WithParameter("@assetId", assetId);
                    }

                    // Create query options with max item limit
                    var queryOptions = new QueryRequestOptions
                    {
                        MaxItemCount = 1000
                    };

                    // Add partition key if assetId is provided
                    if (!string.IsNullOrEmpty(assetId))
                    {
                        queryOptions.PartitionKey = new PartitionKey(assetId);
                    }

                    _logger.LogDebug("Executing range query: DataType={DataType}, AssetClass={AssetClass}, " +
                                   "AssetId={AssetId}, FromDate={FromDate}, ToDate={ToDate}",
                                   dataType, assetClass, assetId ?? "any",
                                   effectiveFromDate.ToString("yyyy-MM-dd"),
                                   effectiveToDate.ToString("yyyy-MM-dd"));

                    var iterator = _container.GetItemQueryIterator<T>(
                        queryDefinition,
                        requestOptions: queryOptions);

                    var results = new List<T>();

                    while (iterator.HasMoreResults && results.Count < 1000)
                    {
                        var response = await iterator.ReadNextAsync(ct);
                        results.AddRange(response);

                        if (results.Count >= 1000)
                        {
                            _logger.LogWarning("Query result was truncated at 1000 items. Consider refining your query criteria.");
                            break;
                        }
                    }

                    return results;
                },
                "QueryByRange",
                cancellationToken
            );
        }

        public async Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (predicate != null)
                {
                    IQueryable<T> query = _container.GetItemLinqQueryable<T>().Where(predicate);

                    // Convert to feed iterator and count items across all pages
                    var feedIterator = query.ToFeedIterator();
                    int count = 0;

                    while (feedIterator.HasMoreResults)
                    {
                        var response = await feedIterator.ReadNextAsync(cancellationToken);
                        count += response.Count;
                    }

                    return count;
                }

                // For counting all items, use a more efficient approach
                var queryDefinition = new QueryDefinition("SELECT VALUE COUNT(1) FROM c");
                var countIterator = _container.GetItemQueryIterator<int>(queryDefinition);

                while (countIterator.HasMoreResults)
                {
                    var response = await countIterator.ReadNextAsync(cancellationToken);
                    return response.FirstOrDefault();
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            return entity != null;
        }

        public async Task<IEnumerable<T>> GetAllVersionsAsync(string baseId, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync<T>(
                async (ct) =>
                {
                    var query = $"SELECT * FROM c WHERE STARTSWITH(c.id, @baseId)";
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@baseId", baseId + "__");

                    var iterator = _container.GetItemQueryIterator<T>(queryDefinition);
                    var results = new List<T>();

                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync(ct);
                        results.AddRange(response);
                    }
                    return results;
                },
                "GetAllVersions",
                cancellationToken
            );
        }

        public async Task<int> PurgeSoftDeletedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Use SQL query to get soft deleted items
                var query = "SELECT * FROM c WHERE c.isDeleted = true";
                var iterator = _container.GetItemQueryIterator<T>(new QueryDefinition(query));

                var toDelete = new List<T>();
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);
                    toDelete.AddRange(response);
                }

                int count = 0;
                foreach (var entity in toDelete)
                {
                    var partitionKey = GetPartitionKey(entity);
                    await _container.DeleteItemAsync<T>(entity.Id, partitionKey, cancellationToken: cancellationToken);
                    count++;
                }
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purging soft-deleted entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }
    }
}