using System;
using System.Threading;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using Phoenix.MarketData.Core.Events;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Infrastructure.Repositories;

namespace Phoenix.MarketData.Infrastructure.Tests.Repository.Base
{
    public abstract class BaseMarketDataRepositoryTests
    {
        protected readonly Mock<Container> MockContainer;
        protected readonly Mock<ILogger<CosmosRepository<FxSpotPriceData>>> MockLogger;
        protected readonly Mock<IEventPublisher> MockEventPublisher;
        protected readonly CosmosRepository<FxSpotPriceData> Repository;
        protected readonly string Id;
        protected readonly string PartitionKey;
        protected readonly FxSpotPriceData MarketData;

        protected BaseMarketDataRepositoryTests()
        {
            MockContainer = new Mock<Container>();
            MockLogger = new Mock<ILogger<CosmosRepository<FxSpotPriceData>>>();
            MockEventPublisher = new Mock<IEventPublisher>();
            
            Repository = new CosmosRepository<FxSpotPriceData>(
                MockContainer.Object, 
                MockLogger.Object, 
                MockEventPublisher.Object);

            // Define these first to match the original file structure
            Id = "price.spot__fx__eurusd__global__2025-05-14__official__1";
            PartitionKey = "eurusd";
            
            // Initialize MarketData but don't set Id directly as it's calculated from properties
            MarketData = new FxSpotPriceData
            {
                Price = 1.1m,
                Version = 1,
                SchemaVersion = "0.0.0",
                AssetId = "eurusd",
                AssetClass = "fx",
                DataType = "price.spot",
                Region = "global",
                DocumentType = "official",
                AsOfDate = new DateOnly(2025, 5, 14)
            };
            
            // Note: Id is computed automatically based on the properties above
            // so we don't need to set it explicitly
        }
    }
}