using Microsoft.Azure.Cosmos;
using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Infrastructure.Cosmos
{
    public class MarketDataRepository
    {
        private readonly Container _container;
        private VersionManager _versionManager;

        public MarketDataRepository(CosmosClient cosmosClient, string databaseId, string containerId)
        {
            _container = cosmosClient.GetContainer(databaseId, containerId);
            _versionManager = new VersionManager(this);
        }

        /// <summary>
        /// Saves a market data object asynchronously to the configured Cosmos DB container.
        /// </summary>
        /// <typeparam name="T">The type of the market data object, implementing IMarketDataObject interface.</typeparam>
        /// <param name="marketData">The market data object to be saved.</param>
        /// <param name="saveNextVersion">Boolean indicating whether to save to the next version (latest) of the
        /// market data object</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SaveAsync<T>(T marketData, bool saveNextVersion = true) where T : IMarketDataObject
        {
            // If saving to the next (latest) version then we need to update the version of the object by retrieving it
            // via the version manager
            if (saveNextVersion)
            {
                marketData.Version = await _versionManager.GetNextVersionAsync<T>(marketData.DataType, marketData.AssetClass,
                    marketData.AssetId, marketData.Region, marketData.AsOfDate, marketData.DocumentType);
            }
            
            await _container.CreateItemAsync(marketData, new PartitionKey(marketData.AssetId));
        }

        public async Task<T?> GetLatestAsync<T>(string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType) where T : IMarketDataObject
        {
            var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.assetId = @assetId AND c.assetClass = @assetClass AND c.region = @region AND c.dataType = @dataType AND c.documentType = @documentType AND c.asOfDate = @asOfDate ORDER BY c.version DESC")
                .WithParameter("@assetId", assetId)
                .WithParameter("@assetClass", assetClass)
                .WithParameter("@region", region)
                .WithParameter("@dataType", dataType)
                .WithParameter("@documentType", documentType)
                .WithParameter("@asOfDate", asOfDate);

            using var feedIterator = _container.GetItemQueryIterator<T>(query, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(assetId),
                MaxBufferedItemCount = 1,
                MaxItemCount = 1
            });

            if (!feedIterator.HasMoreResults) return default;
            var response = await feedIterator.ReadNextAsync();
            return response.FirstOrDefault();
        }
    }
}