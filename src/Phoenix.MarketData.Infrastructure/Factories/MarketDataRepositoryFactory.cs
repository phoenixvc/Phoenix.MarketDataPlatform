using System;
using Microsoft.Azure.Cosmos;
using Phoenix.MarketData.Domain.Repositories;
using Phoenix.MarketData.Infrastructure.Cosmos;

namespace Phoenix.MarketData.Infrastructure.Factories
{
    public class MarketDataRepositoryFactory : IMarketDataRepositoryFactory
    {
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseId;
        private readonly string _containerId;

        public MarketDataRepositoryFactory(
            CosmosClient cosmosClient,
            string databaseId,
            string containerId)
        {
            _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
            _databaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
            _containerId = containerId ?? throw new ArgumentNullException(nameof(containerId));
        }

        public IMarketDataRepository CreateRepository()
        {
            var container = _cosmosClient.GetContainer(_databaseId, _containerId);
            return new MarketDataRepository(container);
        }
    }
}