using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Repositories
{
    // File containing partition key related methods
    public partial class CosmosRepository<T> where T : class, IMarketDataEntity
    {
        /// <summary>
        /// Gets the partition key for a specific entity ID.
        /// This is a fallback method when we only have the ID but not the full entity.
        /// </summary>
        private async Task<PartitionKey> GetPartitionKeyForIdAsync(string id, CancellationToken cancellationToken = default)
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
                    try
                    {
                        var queryOptions = new QueryRequestOptions
                        {
                            PartitionKey = new PartitionKey(potentialKey)
                        };

                        var queryDefinition = new QueryDefinition(
                            "SELECT * FROM c WHERE c.id = @id")
                            .WithParameter("@id", id);

                        var iterator = _container.GetItemQueryIterator<T>(queryDefinition, requestOptions: queryOptions);
                        if (iterator == null)
                        {
                            _logger.LogWarning("Query iterator is null for ID {Id} with potential key {Key}", id, potentialKey);
                            continue;
                        }

                        while (iterator.HasMoreResults)
                        {
                            var response = await iterator.ReadNextAsync(cancellationToken);
                            if (response != null && response.Count > 0)
                            {
                                var entity = response.FirstOrDefault();
                                if (entity != null)
                                {
                                    return new PartitionKey(_partitionKeyResolver(entity));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error trying partition key {Key} for ID {Id}", potentialKey, id);
                        // Continue to the next potential key
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
    }
}