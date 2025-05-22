using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Core.Events;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Repositories
{
    // File containing CRUD methods
    public partial class CosmosRepository<T> where T : class, IMarketDataEntity
    {
        public async Task<T> GetByIdOrThrowAsync(string id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity == null)
                throw new EntityNotFoundException(typeof(T).Name, id);
            return entity;
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var partitionKey = GetPartitionKey(entity);
                var response = await _container.CreateItemAsync(entity, partitionKey, cancellationToken: cancellationToken);
                if (_eventPublisher != null)
                    await _eventPublisher.PublishAsync(
                        new EntityCreatedEvent<T>(entity),
                        GetEventTopicName("created"),
                        cancellationToken);
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var partitionKey = GetPartitionKey(entity);
                var response = await _container.UpsertItemAsync(entity, partitionKey, cancellationToken: cancellationToken);
                if (_eventPublisher != null)
                    await _eventPublisher.PublishAsync(
                        new EntityUpdatedEvent<T>(entity),
                        GetEventTopicName("updated"),
                        cancellationToken);
                return response.Resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(T).Name);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id, bool soft = false, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("DeleteAsync called with ID {Id}, soft={SoftDelete}", id, soft);

            if (soft && typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
            {
                // Soft delete: set IsDeleted = true and update
                try
                {
                    // First get the entity
                    var entity = await GetByIdAsync(id, cancellationToken);
                    if (entity == null)
                    {
                        _logger.LogWarning("Entity not found for soft delete: {Id}", id);
                        return false;
                    }

                    // Cast and set the IsDeleted flag
                    var softDeletable = entity as ISoftDeletable;
                    if (softDeletable == null)
                    {
                        _logger.LogWarning("Entity does not implement ISoftDeletable: {Id}", id);
                        return false;
                    }

                    softDeletable.IsDeleted = true;
                    _logger.LogDebug("Set IsDeleted=true for entity {Id}", id);

                    // Use UpsertItemAsync
                    var partitionKey = GetPartitionKey(entity);
                    _logger.LogDebug("Upserting soft-deleted entity with ID {Id}", id);
                    await _container.UpsertItemAsync(entity, partitionKey, cancellationToken: cancellationToken);

                    // Publish event
                    if (_eventPublisher != null)
                    {
                        await _eventPublisher.PublishAsync(
                            new EntityDeletedEvent(id, typeof(T).Name),
                            GetEventTopicName("deleted"),
                            cancellationToken);
                    }

                    return true;
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Attempted to soft-delete non-existent entity {EntityType} with ID {Id}", typeof(T).Name, id);
                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error performing soft delete for entity {Id}", id);
                    throw;
                }
            }
            else
            {
                // Hard delete implementation
                _logger.LogDebug("Performing hard delete for ID {Id}", id);
                try
                {
                    var partitionKey = await GetPartitionKeyForIdAsync(id, cancellationToken);
                    await _container.DeleteItemAsync<T>(id, partitionKey, cancellationToken: cancellationToken);

                    if (_eventPublisher != null)
                    {
                        await _eventPublisher.PublishAsync(
                            new EntityDeletedEvent(id, typeof(T).Name),
                            GetEventTopicName("deleted"),
                            cancellationToken);
                    }

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
        }

        private string GetEventTopicName(string eventType)
        {
            return $"{typeof(T).Name.ToLowerInvariant()}-{eventType}";
        }
    }
}