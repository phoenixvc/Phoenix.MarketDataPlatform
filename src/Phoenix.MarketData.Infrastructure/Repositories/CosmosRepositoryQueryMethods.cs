using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Repositories
{
    // File containing query methods
    public partial class CosmosRepository<T> where T : class, IMarketDataEntity
    {
        /// <summary>
        /// Helper method to execute a query with consistent error handling
        /// </summary>
        private async Task<IEnumerable<TResult>> ExecuteQueryAsync<TResult>(
            Func<CancellationToken, Task<IEnumerable<TResult>>> queryFunc,
            string operationName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var results = await queryFunc(cancellationToken);
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
            Func<CancellationToken, Task<TResult?>> queryFunc,
            string operationName,
            string? entityId = null,
            CancellationToken cancellationToken = default)
            where TResult : class
        {
            try
            {
                var result = await queryFunc(cancellationToken);
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

        // Rest of query methods (GetAllAsync, GetPagedAsync, QueryAsync, etc.)
        public async Task<IEnumerable<T>> GetAllAsync(bool includeSoftDeleted = false, CancellationToken cancellationToken = default)
        {
            return await ExecuteQueryAsync<T>(
                async (ct) =>
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
                        var response = await iterator.ReadNextAsync(ct);
                        results.AddRange(response);
                    }
                    return results;
                },
                "GetAll",
                cancellationToken
            );
        }

        public async Task<IEnumerable<T>> QueryAsync(
            Expression<Func<T, bool>> predicate,
            bool includeSoftDeleted = false,
            CancellationToken cancellationToken = default)
        {
            var results = new List<T>();

            try
            {
                // Try the LINQ approach first
                var queryable = _container.GetItemLinqQueryable<T>();
                var query = queryable.Where(predicate);

                using var feedIterator = query.ToFeedIterator();

                while (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync(cancellationToken);

                    // Filter out soft-deleted items if needed
                    if (!includeSoftDeleted && typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
                    {
                        results.AddRange(response.Where(e => !(e as ISoftDeletable)!.IsDeleted));
                    }
                    else
                    {
                        results.AddRange(response);
                    }
                }

                _logger.LogDebug("QueryAsync returned {Count} results", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "LINQ query failed, trying fallback approach");

                // Fall back to a direct query if LINQ fails
                var queryDefinition = new QueryDefinition("SELECT * FROM c");
                using var iterator = _container.GetItemQueryIterator<T>(queryDefinition);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);

                    // Apply the predicate and filter
                    var filtered = response.AsQueryable().Where(predicate.Compile());

                    // Apply soft delete filtering
                    if (!includeSoftDeleted && typeof(ISoftDeletable).IsAssignableFrom(typeof(T)))
                    {
                        filtered = filtered.Where(e => !(e as ISoftDeletable)!.IsDeleted);
                    }

                    results.AddRange(filtered);
                }

                _logger.LogDebug("Fallback query returned {Count} results", results.Count);
                return results;
            }
        }
    }
}