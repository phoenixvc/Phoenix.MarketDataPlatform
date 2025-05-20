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
    // ========== EXTENSIONS ==========
    public static class CosmosRepositoryExtensions
    {
        public static async Task<IEnumerable<T>> QueryByPredicateAsync<T>(
            this CosmosRepository<T> repo,
            Func<IQueryable<T>, IQueryable<T>> predicateBuilder,
            bool includeSoftDeleted = false)
            where T : class, IVersionedMarketDataEntity
        {
            var container = repo.GetContainer();
            var query = predicateBuilder(container.GetItemLinqQueryable<T>());
            var iterator = query.ToFeedIterator();

            var results = new List<T>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                if (typeof(ISoftDeletable).IsAssignableFrom(typeof(T)) && !includeSoftDeleted)
                    results.AddRange(response.Where(e => !(e as ISoftDeletable)!.IsDeleted));
                else
                    results.AddRange(response);
            }
            return results;
        }

        public static async Task<T?> GetByVersionAsync<T>(
            this CosmosRepository<T> repo,
            string id,
            int version)
            where T : class, IEntity, IVersionedMarketDataEntity
        {
            var container = repo.GetContainer();
            var query = container.GetItemLinqQueryable<T>()
                .Where(e => e.Id == id && e.Version == version)
                .ToFeedIterator();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                var entity = response.FirstOrDefault();
                if (entity != null && (!(entity is ISoftDeletable sd) || !sd.IsDeleted))
                    return entity;
            }
            return null;
        }
    }
}