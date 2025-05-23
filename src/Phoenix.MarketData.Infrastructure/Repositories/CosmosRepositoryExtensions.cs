using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Domain.Events;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Repositories
{
    // ========== EXTENSIONS ==========
    public static class CosmosRepositoryExtensions
    {
        private static async Task<List<T>> ExecuteCosmosQueryAsync<T>(
            Container container,
            FeedIterator<T> iterator,
            bool includeSoftDeleted,
            ILogger? logger = null)
            where T : class
        {
            var results = new List<T>();
            try
            {
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    if (!includeSoftDeleted && typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
                    {
                        results.AddRange(response.Where(e =>
                        {
                            var sd = e as ISoftDeletable;
                            return sd == null || !sd.IsDeleted;
                        }));
                    }
                    else
                    {
                        results.AddRange(response);
                    }
                }
                logger?.LogDebug("Successfully retrieved {Count} items of type {EntityType}", results.Count, typeof(T).Name);
            }
            catch (CosmosException ce)
            {
                logger?.LogError(ce, "Cosmos DB error occurred during query: {StatusCode}. Error: {ErrorMessage}", ce.StatusCode, ce.Message);
                throw;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error querying entities of type {EntityType}: {ErrorMessage}", typeof(T).Name, ex.Message);
                throw;
            }
            return results;
        }

        public static async Task<IEnumerable<T>> QueryByPredicateAsync<T>(
            this CosmosRepository<T> repo,
            Func<IQueryable<T>, IQueryable<T>> predicateBuilder,
            bool includeSoftDeleted = false,
            ILogger? logger = null)
            where T : class, IMarketDataEntity
        {
            var container = repo.GetContainer();
            var query = predicateBuilder(container.GetItemLinqQueryable<T>());
            var iterator = query.ToFeedIterator();
            return await ExecuteCosmosQueryAsync(container, iterator, includeSoftDeleted, logger);
        }

        public static async Task<T?> GetByVersionAsync<T>(
            this CosmosRepository<T> repo,
            string id,
            int version,
            ILogger? logger = null)
            where T : class, IEntity, IMarketDataEntity
        {
            var container = repo.GetContainer();
            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.id = @id AND c.version = @version")
                .WithParameter("@id", id)
                .WithParameter("@version", version);
            var iterator = container.GetItemQueryIterator<T>(queryDefinition);
            var results = await ExecuteCosmosQueryAsync(container, iterator, false, logger);
            var entity = results.FirstOrDefault();
            if (entity != null && !(entity is ISoftDeletable sd && sd.IsDeleted))
            {
                logger?.LogDebug("Found entity of type {EntityType} with id {Id} and version {Version}", typeof(T).Name, id, version);
                return entity;
            }
            logger?.LogDebug("Entity of type {EntityType} with id {Id} and version {Version} not found or soft-deleted", typeof(T).Name, id, version);
            return null;
        }
    }
}