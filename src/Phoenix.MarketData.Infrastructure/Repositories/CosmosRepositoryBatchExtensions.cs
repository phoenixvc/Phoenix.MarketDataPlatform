using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Repositories
{
    /// <summary>
    /// Extension methods for batch operations on CosmosRepository
    /// </summary>
    public static class CosmosRepositoryBatchExtensions
    {
        /// <summary>
        /// Adds multiple entities in sequence
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="repository">Repository instance</param>
        /// <param name="items">Collection of entities to add</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Collection of added entities</returns>
        public static async Task<IEnumerable<T>> AddBatchAsync<T>(
            this CosmosRepository<T> repository, 
            IEnumerable<T> items,
            CancellationToken cancellationToken = default) 
            where T : class, IMarketDataEntity
        {
            var results = new List<T>();
            foreach (var item in items)
            {
                var result = await repository.AddAsync(item, cancellationToken);
                results.Add(result);
            }
            return results;
        }
        
        /// <summary>
        /// Updates multiple entities in sequence
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="repository">Repository instance</param>
        /// <param name="items">Collection of entities to update</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Collection of updated entities</returns>
        public static async Task<IEnumerable<T>> UpdateBatchAsync<T>(
            this CosmosRepository<T> repository, 
            IEnumerable<T> items,
            CancellationToken cancellationToken = default) 
            where T : class, IMarketDataEntity
        {
            var results = new List<T>();
            foreach (var item in items)
            {
                var result = await repository.UpdateAsync(item, cancellationToken);
                results.Add(result);
            }
            return results;
        }
        
        /// <summary>
        /// Deletes multiple entities by ID in sequence
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="repository">Repository instance</param>
        /// <param name="ids">Collection of entity IDs to delete</param>
        /// <param name="useSoftDelete">Whether to use soft delete</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Collection of deletion results (true if deleted)</returns>
        public static async Task<IEnumerable<bool>> DeleteBatchAsync<T>(
            this CosmosRepository<T> repository, 
            IEnumerable<string> ids,
            bool useSoftDelete = false,
            CancellationToken cancellationToken = default) 
            where T : class, IMarketDataEntity
        {
            var results = new List<bool>();
            foreach (var id in ids)
            {
                var result = await repository.DeleteAsync(id, useSoftDelete, cancellationToken);
                results.Add(result);
            }
            return results;
        }
    }
}