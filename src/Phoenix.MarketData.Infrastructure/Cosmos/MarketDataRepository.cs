using Microsoft.Azure.Cosmos;
using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Infrastructure.Cosmos
{
    public class MarketDataRepository
    {
        private readonly Container _container;

        public MarketDataRepository(CosmosClient cosmosClient, string databaseId, string containerId)
        {
            _container = cosmosClient.GetContainer(databaseId, containerId);
        }

        public async Task SaveAsync<T>(T marketData) where T : IMarketDataObject
        {
            await _container.CreateItemAsync(marketData, new PartitionKey(marketData.AssetId));
        }

        public async Task<T?> GetLatestAsync<T>(string assetId, string dataType, string documentType, DateTime timestamp) where T : IMarketDataObject
        {
            var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.assetId = @assetId AND c.dataType = @dataType AND c.documentType = @documentType AND c.timestamp = @timestamp ORDER BY c.version DESC")
                .WithParameter("@assetId", assetId)
                .WithParameter("@dataType", dataType)
                .WithParameter("@documentType", documentType)
                .WithParameter("@timestamp", timestamp);

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