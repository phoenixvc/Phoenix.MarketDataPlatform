using System;
using Microsoft.Azure.Cosmos;
using Phoenix.MarketData.Domain.Factories;
using Phoenix.MarketData.Domain.Models.Interfaces;
using Phoenix.MarketData.Domain.Repositories;
using Phoenix.MarketData.Infrastructure.Cosmos;

namespace Phoenix.MarketData.Infrastructure.Factories
{
    public interface IRepositoryFactory
    {
        IRepository<T> Create<T>() where T : IMarketData;
    }

    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseId;
        private readonly string _containerId;

        public RepositoryFactory(CosmosClient cosmosClient, string databaseId, string containerId)
        {
            _cosmosClient = cosmosClient;
            _databaseId = databaseId;
            _containerId = containerId;
        }

        public IRepository<T> Create<T>() where T : IMarketData
        {
            var container = _cosmosClient.GetContainer(_databaseId, _containerId);
            return new Repository<T>(container);
        }
    }
}