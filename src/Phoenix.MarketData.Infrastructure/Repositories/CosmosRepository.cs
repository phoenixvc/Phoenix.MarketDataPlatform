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
                // Try to find the entity to get its partition key
                // Use a direct query with SQL to avoid circular dependency with GetByIdAsync
                var queryDefinition = new QueryDefinition(
                    "SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);

                var iterator = _container.GetItemQueryIterator<T>(queryDefinition);
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    if (response.Count > 0)
                    {
                        return new PartitionKey(_partitionKeyResolver(response.First()));
                    }
                }

                // If we can't find the entity, fall back to using the ID
                // This may fail if the partition key is not the ID
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
        /// Gets the partition key for an entity
        /// </summary>
        private PartitionKey GetPartitionKey(T entity)
        {
            return new PartitionKey(_partitionKeyResolver(entity));
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                // We need to determine the partition key for this ID
                var partitionKey = await GetPartitionKeyForIdAsync(id);

                var response = await _container.ReadItemAsync<T>(id, partitionKey);
                var entity = response.Resource;

                // Soft delete filter
                if (entity is ISoftDeletable sd && sd.IsDeleted)
                    return null;
                return entity;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Entity {EntityType} with ID {Id} not found", typeof(T).Name, id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity {EntityType} with ID {Id}", typeof(T).Name, id);
                throw;
            }
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
            var results = new List<T>();
            try
            {
                var query = _container.GetItemLinqQueryable<T>().ToFeedIterator();
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)) && !includeSoftDeleted)
                        results.AddRange(response.Where(e => !(e as ISoftDeletable)!.IsDeleted));
                    else
                        results.AddRange(response);
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all entities of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Gets a paged collection of entities with continuation token support
        /// </summary>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="continuationToken">Token for continuing a previous query</param>
        /// <param name="includeSoftDeleted">Whether to include soft-deleted items</param>
        /// <returns>A tuple containing items and continuation token for the next page</returns>
        public async Task<(IEnumerable<T> Items, string? ContinuationToken)> GetPagedAsync(
            int pageSize,
            string? continuationToken = null,
            bool includeSoftDeleted = false)
        {
            try
            {
                // For LINQ queries, we need to use FeedIterator approach
                var queryOptions = new QueryRequestOptions
                {
                    MaxItemCount = pageSize
                };

                // Create SQL query directly when using continuation tokens
                string query = "SELECT * FROM c";

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

                    // Filter out soft-deleted items if needed
                    if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)) && !includeSoftDeleted)
                    {
                        results.AddRange(response.Where(e => !(e as ISoftDeletable)!.IsDeleted));
                    }
                    else
                    {
                        results.AddRange(response);
                    }

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

        /// <summary>
        /// Queries entities based on a predicate
        /// </summary>
        /// <param name="predicate">The filter predicate</param>
        /// <param name="includeSoftDeleted">Whether to include soft-deleted items</param>
        /// <returns>A collection of entities matching the predicate</returns>
        public async Task<IEnumerable<T>> QueryAsync(
            Expression<Func<T, bool>> predicate,
            bool includeSoftDeleted = false)
        {
            try
            {
                var query = _container.GetItemLinqQueryable<T>()
                    .Where(predicate);

                var iterator = query.ToFeedIterator();
                var results = new List<T>();

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();

                    // Filter out soft-deleted items if needed
                    if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)) && !includeSoftDeleted)
                    {
                        results.AddRange(response.Where(e => !(e as ISoftDeletable)!.IsDeleted));
                    }
                    else
                    {
                        results.AddRange(response);
                    }
                }

                _logger.LogDebug("Query returned {Count} results", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying entities of type {EntityType}", typeof(T).Name);
                throw;
            }
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
                // Soft delete: set IsDeleted = true and update entity
                var entity = await GetByIdAsync(id);
                if (entity == null) return false;
                ((ISoftDeletable)entity).IsDeleted = true;
                await UpdateAsync(entity);
                if (_eventPublisher != null)
                    await _eventPublisher.PublishAsync(
                        new EntityDeletedEvent(id, typeof(T).Name),
                        GetEventTopicName("deleted"));
                return true;
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

        /// <summary>
        /// Constructs a standardized event topic name for entity events.
        /// </summary>
        /// <param name="eventType">The type of event (e.g., "created", "updated", "deleted")</param>
        /// <returns>A standardized event topic name</returns>
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
            // Use the assetId for the partition key if possible
            var queryOptions = new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(assetId)
            };

            var query = _container.GetItemLinqQueryable<T>(requestOptions: queryOptions)
                .Where(e =>
                    e.DataType == dataType &&
                    e.AssetClass == assetClass &&
                    e.AssetId == assetId &&
                    e.Region == region &&
                    e.AsOfDate == asOfDate &&
                    e.DocumentType == documentType)
                .OrderByDescending(e => ((IVersionedMarketDataEntity)e).Version)
                .Take(1)
                .ToFeedIterator();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                return response.FirstOrDefault();
            }
            return null;
        }

        public async Task<int> GetNextVersionAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType)
        {
            var latest = await GetLatestMarketDataAsync(
                dataType, assetClass, assetId, region, asOfDate, documentType);
            return latest is IVersionedMarketDataEntity v ? v.Version + 1 : 1;
        }

        /// <summary>
        /// Queries entities by date range with optional filtering.
        /// </summary>
        /// <param name="dataType">The data type to filter by</param>
        /// <param name="assetClass">The asset class to filter by</param>
        /// <param name="assetId">Optional asset ID filter</param>
        /// <param name="fromDate">Start date for the query range. If null, a default of 30 days ago will be used to avoid full container scans.</param>
        /// <param name="toDate">End date for the query range. If null, the current date will be used.</param>
        /// <param name="maxItems">Maximum number of items to return (default: 1000)</param>
        /// <returns>A collection of entities matching the criteria</returns>
        /// <remarks>
        /// IMPORTANT: This method uses date bounds to optimize RU consumption and performance.
        /// If no dates are provided, it will default to the last 30 days to prevent expensive full container scans.
        /// </remarks>
        public async Task<IEnumerable<T>> QueryByRangeAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int maxItems = 1000)
        {
            try
            {
                // Set default date bounds if not provided to avoid full container scans
                var effectiveFromDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
                var effectiveToDate = toDate ?? DateTime.UtcNow;

                // Create query options with max item limit for performance
                var queryOptions = new QueryRequestOptions
                {
                    MaxItemCount = maxItems
                };

                // Add partition key if assetId is provided (using assetId as partition key)
                if (!string.IsNullOrEmpty(assetId))
                {
                    queryOptions.PartitionKey = new PartitionKey(assetId);
                }

                var queryable = _container.GetItemLinqQueryable<T>(
                    allowSynchronousQueryExecution: false,
                    requestOptions: queryOptions);

                var query = queryable
                    .Where(e =>
                        (e.DataType == dataType) &&
                        (e.AssetClass == assetClass) &&
                        (assetId == null || e.AssetId == assetId) &&
                        (e.AsOfDate >= DateOnly.FromDateTime(effectiveFromDate)) &&
                        (e.AsOfDate <= DateOnly.FromDateTime(effectiveToDate))
                    );

                _logger.LogDebug("Executing range query: DataType={DataType}, AssetClass={AssetClass}, " +
                               "AssetId={AssetId}, FromDate={FromDate}, ToDate={ToDate}, MaxItems={MaxItems}",
                               dataType, assetClass, assetId ?? "any",
                               effectiveFromDate.ToString("yyyy-MM-dd"),
                               effectiveToDate.ToString("yyyy-MM-dd"),
                               maxItems);

                var iterator = query.ToFeedIterator();
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

                _logger.LogDebug("Query returned {Count} results", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing range query: {ErrorMessage}", ex.Message);
                throw;
            }
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
            var query = _container.GetItemLinqQueryable<T>()
                .Where(e => e.Id.StartsWith(baseId + "__"));
            var iterator = query.ToFeedIterator();

            var results = new List<T>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }
            return results;
        }

        public async Task<int> PurgeSoftDeletedAsync()
        {
            var allSoftDeleted = await GetAllAsync(includeSoftDeleted: true);
            var toDelete = allSoftDeleted.Where(e => (e as ISoftDeletable)?.IsDeleted == true).ToList();
            int count = 0;
            foreach (var entity in toDelete)
            {
                var partitionKey = GetPartitionKey(entity);
                await _container.DeleteItemAsync<T>(entity.Id, partitionKey);
                count++;
            }
            return count;
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