using Microsoft.Azure.Cosmos;

namespace Phoenix.MarketData.Infrastructure.Cosmos
{
    public static class CosmosClientFactory
    {
        private static CosmosClient? _cosmosClient;

        public static CosmosClient CreateClient(string connectionString)
        {
            return _cosmosClient ??= new CosmosClient(connectionString, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                ConsistencyLevel = ConsistencyLevel.Session
            });
        }
    }
}