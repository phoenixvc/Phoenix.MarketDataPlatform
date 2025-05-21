using System;
using System.Collections.Generic;
using System.Linq;
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

        public CosmosRepository(Container container, ILogger<CosmosRepository<T>> logger, IEventPublisher? eventPublisher = null)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventPublisher = eventPublisher;
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(id, new PartitionKey(id));
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

        public async Task<T> AddAsync(T entity)
        {
            try
            {
                var response = await _container.CreateItemAsync(entity);
                if (_eventPublisher != null)
                    await _eventPublisher.PublishAsync(new EntityCreatedEvent<T>(entity), $"{typeof(T).Name.ToLowerInvariant()}-created");
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
                // Add the partition key parameter to match the mock expectations
                var response = await _container.UpsertItemAsync(entity, new PartitionKey(entity.Id));
                if (_eventPublisher != null)
                    await _eventPublisher.PublishAsync(new EntityUpdatedEvent<T>(entity), $"{typeof(T).Name.ToLowerInvariant()}-updated");
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
                    await _eventPublisher.PublishAsync(new EntityDeletedEvent(id, typeof(T).Name), $"{typeof(T).Name.ToLowerInvariant()}-deleted");
                return true;
            }

            // Hard delete
            try
            {
                await _container.DeleteItemAsync<T>(id, new PartitionKey(id));
                if (_eventPublisher != null)
                    await _eventPublisher.PublishAsync(new EntityDeletedEvent(id, typeof(T).Name), $"{typeof(T).Name.ToLowerInvariant()}-deleted");
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

        public async Task<int> BulkInsertAsync(IEnumerable<T> entities)
        {
            int count = 0;
            var exceptions = new List<Exception>();
            foreach (var entity in entities)
            {
                try
                {
                    await _container.CreateItemAsync(entity);
                    count++;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    _logger.LogError(ex, "Bulk insert failed for entity {EntityType}", typeof(T).Name);
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
            var query = _container.GetItemLinqQueryable<T>()
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

        public async Task<IEnumerable<T>> QueryByRangeAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var queryable = _container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution: false);

            // Cast to IQueryable<IMarketData> if possible
            // Make sure T implements IMarketData, or adjust as needed.
            var query = queryable
                .Where(e =>
                    (e.DataType == dataType) &&
                    (e.AssetClass == assetClass) &&
                    (assetId == null || e.AssetId == assetId) &&
                    (
                        (!fromDate.HasValue || e.AsOfDate >= DateOnly.FromDateTime(fromDate.Value)) &&
                        (!toDate.HasValue || e.AsOfDate <= DateOnly.FromDateTime(toDate.Value))
                    ));

            var iterator = query.ToFeedIterator();
            var results = new List<T>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }
            return results;
        }

        public async Task<int> CountAsync(Func<IQueryable<T>, IOrderedQueryable<T>>? predicateBuilder = null)
        {
            var query = _container.GetItemLinqQueryable<T>();
            if (predicateBuilder != null)
                query = predicateBuilder(query);

            // Cosmos DB count pattern:
            var countQuery = query.Select(_ => 1);
            var iterator = countQuery.ToFeedIterator();
            int count = 0;
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                count += response.Count();
            }
            return count;
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
                await _container.DeleteItemAsync<T>(entity.Id, new PartitionKey(entity.Id));
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
