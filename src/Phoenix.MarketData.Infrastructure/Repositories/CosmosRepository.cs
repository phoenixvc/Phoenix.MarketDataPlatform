using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Core.Events;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Repositories
{
    // ========== MAIN REPOSITORY ==========
    public class CosmosRepository<T> : IRepository<T> where T : class, IMarketDataEntity
    {
        private readonly Container _container;
        private readonly ILogger<CosmosRepository<T>> _logger;
        private readonly IEventPublisher? _eventPublisher;
        private readonly Func<T, string> _partitionKeyResolver;

        public CosmosRepository(
            Container container,
            ILogger<CosmosRepository<T>> logger,
            IEventPublisher? eventPublisher = null,
            Func<T, string>? partitionKeyResolver = null)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventPublisher = eventPublisher;
            _partitionKeyResolver = partitionKeyResolver ?? (e => e.AssetId);
        }

        /// <summary>
        /// Gets the partition key for a specific entity ID.
        /// This is a fallback method when we only have the ID but not the full entity.
        /// </summary>
        private async Task<PartitionKey> GetPartitionKeyForIdAsync(string id)
        {
            try
            {
                // First attempt: If ID format contains partition info, extract it
                string possiblePartitionKey = ExtractPossiblePartitionKeyFromId(id);
                if (!string.IsNullOrEmpty(possiblePartitionKey))
                {
                    return new PartitionKey(possiblePartitionKey);
                }

                // Try known partition key values
                var likelyPartitionKeys = GetLikelyPartitionKeysForId(id);

                foreach (var potentialKey in likelyPartitionKeys)
                {
                    var queryOptions = new QueryRequestOptions
                    {
                        PartitionKey = new PartitionKey(potentialKey)
                    };

                    var queryDefinition = new QueryDefinition(
                        "SELECT * FROM c WHERE c.id = @id")
                        .WithParameter("@id", id);

                    var iterator = _container.GetItemQueryIterator<T>(queryDefinition, requestOptions: queryOptions);

                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        if (response.Count > 0)
                        {
                            return new PartitionKey(_partitionKeyResolver(response.First()));
                        }
                    }
                }

                // If we can't find the entity, fall back to using the ID
                _logger.LogWarning("Could not determine partition key for entity with ID {Id}, falling back to ID", id);
                return new PartitionKey(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining partition key for entity with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Extract a possible partition key from the ID if the ID follows a known pattern
        /// </summary>
        private string ExtractPossiblePartitionKeyFromId(string id)
        {
            // Example: If IDs are in format "assetId__otherParts"
            if (id.Contains("__"))
            {
                return id.Split("__")[0];
            }

            return string.Empty;
        }

        /// <summary>
        /// Get a list of likely partition keys to check for an entity with the given ID
        /// </summary>
        private List<string> GetLikelyPartitionKeysForId(string id)
        {
            // This is domain-specific logic. Adjust based on your data patterns.
            return new List<string>
            {
                id,                            // The ID itself
                id.Split('_').FirstOrDefault() ?? string.Empty,  // First part of ID if delimited
                id.Split('.').FirstOrDefault() ?? string.Empty   // First part if using dot notation
            }.Where(k => !string.IsNullOrEmpty(k)).Distinct().ToList();
        }

        /// <summary>
        /// Gets the partition key for an entity
        /// </summary>
        private PartitionKey GetPartitionKey(T entity)
        {
            return new PartitionKey(_partitionKeyResolver(entity));
        }

        /// <summary>
        /// Helper method to execute a query with consistent error handling
        /// </summary>
        private async Task<IEnumerable<TResult>> ExecuteQueryAsync<TResult>(
            Func<Task<IEnumerable<TResult>>> queryFunc,
            string operationName)
        {
            try
            {
                var results = await queryFunc();
                _logger.LogDebug("{OperationName} returned {Count} results of type {EntityType}",
                    operationName, results.Count(), typeof(TResult).Name);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing {OperationName} for type {EntityType}: {ErrorMessage}",
                    operationName, typeof(TResult).Name, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Helper method to execute a single item query with consistent error handling
        /// </summary>
        private async Task<TResult?> ExecuteSingleQueryAsync<TResult>(
            Func<Task<TResult?>> queryFunc,
            string operationName,
            string? entityId = null)
            where TResult : class
        {
            try
            {
                var result = await queryFunc();
                if (result == null)
                {
                    _logger.LogInformation("{OperationName} for {EntityType}{IdInfo} returned no result",
                        operationName, typeof(TResult).Name,
                        entityId == null ? "" : $" with ID {entityId}");
                }
                return result;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("{OperationName} for {EntityType}{IdInfo} not found",
                    operationName, typeof(TResult).Name,
                    entityId == null ? "" : $" with ID {entityId}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing {OperationName} for {EntityType}{IdInfo}: {ErrorMessage}",
                    operationName, typeof(TResult).Name,
                    entityId == null ? "" : $" with ID {entityId}",
                    ex.Message);
                throw;
            }
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            return await ExecuteSingleQueryAsync<T>(
                async () =>
                {
                    var partitionKey = await GetPartitionKeyForIdAsync(id);
                    var response = await _container.ReadItemAsync<T>(id, partitionKey);
                    var entity = response.Resource;

                    // Soft delete filter
                    if (entity is ISoftDeletable sd && sd.IsDeleted)
                        return null;
                    return entity;
                },
                "GetById",
                id
            );
        }

        public async Task<T> GetByIdOrThrowAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new EntityNotFoundException(typeof(T).Name, id);
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool includeSoftDeleted = false)
        {
            return await ExecuteQueryAsync<T>(
                async () =>
                {
                    var results = new List<T>();

                    // Use SQL query with soft delete filter if needed
                    string query = "SELECT * FROM c";
                    if (!includeSoftDeleted && typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
                    {
                        query += " WHERE NOT IS_DEFINED(c.isDeleted) OR c.isDeleted = false";
                    }

                    var queryDefinition = new QueryDefinition(query);
                    var iterator = _container.GetItemQueryIterator<T>(queryDefinition);

                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        results.AddRange(response);
                    }
                    return results;
                },
                "GetAll"
            );
        }

        public async Task<(IEnumerable<T> Items, string? ContinuationToken)> GetPagedAsync(
            int pageSize,
            string? continuationToken = null,
            bool includeSoftDeleted = false)
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
                    var response = await feedIterator.ReadNextAsync();
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

        public async Task<IEnumerable<T>> QueryAsync(
            Expression<Func<T, bool>> predicate,
            bool includeSoftDeleted = false)
        {
            return await ExecuteQueryAsync<T>(
                async () =>
                {
                    var results = new List<T>();

                    // Build the LINQ query
                    IQueryable<T> query = _container.GetItemLinqQueryable<T>()
                        .Where(predicate);

                    // Handle soft delete filtering through post-processing
                    // since we can't use dynamic in LINQ expressions
                    var iterator = query.ToFeedIterator();

                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();

                        // Filter soft-deleted items if needed
                        if (!includeSoftDeleted && typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
                        {
                            results.AddRange(response.Where(e => !(e as ISoftDeletable)!.IsDeleted));
                        }
                        else
                        {
                            results.AddRange(response);
                        }
                    }
                    return results;
                },
                "Query"
            );
        }

        public async Task<T> AddAsync(T entity)
        {
            try
            {
                var partitionKey = GetPartitionKey(entity);
                var response = await _container.CreateItemAsync(entity, partitionKey);
                if (_eventPublisher != null)
                    await _eventPublisher.PublishAsync(
                        new EntityCreatedEvent<T>(entity),
                        GetEventTopicName("created"));
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<T> UpdateAsync(T entity)
        {
            try
            {
                var partitionKey = GetPartitionKey(entity);
                var response = await _container.UpsertItemAsync(entity, partitionKey);
                if (_eventPublisher != null)
                    await _eventPublisher.PublishAsync(
                        new EntityUpdatedEvent<T>(entity),
                        GetEventTopicName("updated"));
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id, bool soft = false)
        {
            if (soft && typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
            {
                // Soft delete: set IsDeleted = true and update directly without firing an UpdateAsync event
                try
                {
                    var entity = await GetByIdAsync(id);
                    if (entity == null) return false;

                    // Set the IsDeleted flag
                    ((ISoftDeletable)entity).IsDeleted = true;

                    // Use ReplaceItemAsync directly to avoid firing the UpdateAsync event
                    var partitionKey = GetPartitionKey(entity);
                    await _container.ReplaceItemAsync(entity, id, partitionKey);

                    // Only publish the delete event, not the update event
                    if (_eventPublisher != null)
                        await _eventPublisher.PublishAsync(
                            new EntityDeletedEvent(id, typeof(T).Name),
                            GetEventTopicName("deleted"));

                    return true;
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Attempted to soft-delete non-existent entity {EntityType} with ID {Id}", typeof(T).Name, id);
                    return false;
                }
            }

            // Hard delete
            try
            {
                // We need to determine the partition key for this ID
                var partitionKey = await GetPartitionKeyForIdAsync(id);

                await _container.DeleteItemAsync<T>(id, partitionKey);
                if (_eventPublisher != null)
                    await _eventPublisher.PublishAsync(
                        new EntityDeletedEvent(id, typeof(T).Name),
                        GetEventTopicName("deleted"));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Attempted to delete non-existent entity {EntityType} with ID {Id}", typeof(T).Name, id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
        }

        private string GetEventTopicName(string eventType)
        {
            return $"{typeof(T).Name.ToLowerInvariant()}-{eventType}";
        }

        public async Task<int> BulkInsertAsync(IEnumerable<T> entities)
        {
            int count = 0;
            var exceptions = new List<Exception>();
            foreach (var entity in entities)
            {
                try
                {
                    var partitionKey = GetPartitionKey(entity);
                    await _container.CreateItemAsync(entity, partitionKey);
                    count++;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    _logger.LogError(ex, "Bulk insert failed for entity {EntityType} with ID {Id}: {ErrorMessage}",
                        typeof(T).Name, entity.Id, ex.Message);
                }
            }
            if (exceptions.Count > 0)
                throw new AggregateException($"Bulk insert completed with {exceptions.Count} failures.", exceptions);
            return count;
        }

        public async Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType)
        {
            // Check if T implements IVersionedMarketDataEntity
            if (!typeof(IVersionedMarketDataEntity).IsAssignableFrom(typeof(T)))
            {
                throw new InvalidOperationException(
                    $"Type {typeof(T).Name} must implement IVersionedMarketDataEntity to use GetLatestMarketDataAsync");
            }

            return await ExecuteSingleQueryAsync<T>(
                async () =>
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
                        var response = await iterator.ReadNextAsync();
                        return response.FirstOrDefault();
                    }
                    return null;
                },
                "GetLatestMarketData"
            );
        }

        public async Task<int> GetNextVersionAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType)
        {
            var latest = await GetLatestMarketDataAsync(
                dataType, assetClass, assetId, region, asOfDate, documentType);
            return latest is IVersionedMarketDataEntity v ? v.Version + 1 : 1;
        }

        public async Task<IEnumerable<T>> QueryByRangeAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int maxItems = 1000)
        {
            return await ExecuteQueryAsync<T>(
                async () =>
                {
                    // Set default date bounds if not provided to avoid full container scans
                    var effectiveFromDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                    var effectiveToDate = toDate ?? DateTime.UtcNow;

                    // Build SQL query to avoid dynamic in expression trees
                    var queryBuilder = new System.Text.StringBuilder();
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
                        MaxItemCount = maxItems
                    };

                    // Add partition key if assetId is provided
                    if (!string.IsNullOrEmpty(assetId))
                    {
                        queryOptions.PartitionKey = new PartitionKey(assetId);
                    }

                    _logger.LogDebug("Executing range query: DataType={DataType}, AssetClass={AssetClass}, " +
                                   "AssetId={AssetId}, FromDate={FromDate}, ToDate={ToDate}, MaxItems={MaxItems}",
                                   dataType, assetClass, assetId ?? "any",
                                   effectiveFromDate.ToString("yyyy-MM-dd"),
                                   effectiveToDate.ToString("yyyy-MM-dd"),
                                   maxItems);

                    var iterator = _container.GetItemQueryIterator<T>(
                        queryDefinition,
                        requestOptions: queryOptions);

                    var results = new List<T>();

                    while (iterator.HasMoreResults && results.Count < maxItems)
                    {
                        var response = await iterator.ReadNextAsync();
                        results.AddRange(response);

                        if (results.Count >= maxItems)
                        {
                            _logger.LogWarning("Query result was truncated at {MaxItems} items. Consider refining your query criteria.",
                                maxItems);
                            break;
                        }
                    }

                    return results;
                },
                "QueryByRange"
            );
        }

        public async Task<int> CountAsync(Func<IQueryable<T>, IOrderedQueryable<T>>? predicateBuilder = null)
        {
            try
            {
                var query = _container.GetItemLinqQueryable<T>();
                if (predicateBuilder != null)
                {
                    query = predicateBuilder(query);
                }

                // Convert to feed iterator and count items across all pages
                var feedIterator = query.ToFeedIterator();
                int count = 0;

                while (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync();
                    count += response.Count;
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var entity = await GetByIdAsync(id);
            return entity != null;
        }

        public async Task<IEnumerable<T>> GetAllVersionsAsync(string baseId)
        {
            return await ExecuteQueryAsync<T>(
                async () =>
                {
                    var query = $"SELECT * FROM c WHERE STARTSWITH(c.id, @baseId)";
                    var queryDefinition = new QueryDefinition(query)
                        .WithParameter("@baseId", baseId + "__");

                    var iterator = _container.GetItemQueryIterator<T>(queryDefinition);
                    var results = new List<T>();

                    while (iterator.HasMoreResults)
                    {
                        var response = await iterator.ReadNextAsync();
                        results.AddRange(response);
                    }
                    return results;
                },
                "GetAllVersions"
            );
        }

        public async Task<int> PurgeSoftDeletedAsync()
        {
            try
            {
                // Use SQL query to get soft deleted items
                var query = "SELECT * FROM c WHERE c.isDeleted = true";
                var iterator = _container.GetItemQueryIterator<T>(new QueryDefinition(query));

                var toDelete = new List<T>();
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    toDelete.AddRange(response);
                }

                int count = 0;
                foreach (var entity in toDelete)
                {
                    var partitionKey = GetPartitionKey(entity);
                    await _container.DeleteItemAsync<T>(entity.Id, partitionKey);
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

        internal Container GetContainer() => _container;
    }

    // ========== EXCEPTIONS ==========
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, string id)
            : base($"{entityName} with ID '{id}' was not found.")
        {
        }
    }
}