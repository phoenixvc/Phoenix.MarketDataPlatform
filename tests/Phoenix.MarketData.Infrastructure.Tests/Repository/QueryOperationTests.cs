using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Moq;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Infrastructure.Tests.Repository.Base;
using Xunit;

namespace Phoenix.MarketData.Infrastructure.Tests.Repository
{
    public class QueryOperationTests : BaseMarketDataRepositoryTests
    {
        [Fact]
        public async Task QueryAsync_ShouldReturnFilteredResults()
        {
            // Arrange
            var data = new List<FxSpotPriceData>
            {
                MarketData,
                new FxSpotPriceData
                {
                    AssetId = "gbpusd",
                    Price = 1.3m,
                    SchemaVersion = "0.0.0",
                    AssetClass = "fx",
                    DataType = "price.spot",
                    Region = "global",
                    DocumentType = "official",
                    AsOfDate = new DateOnly(2025, 5, 14)
                }
            };

            var mockFeedIterator = new Mock<FeedIterator<FxSpotPriceData>>();
            mockFeedIterator
                .SetupSequence(f => f.HasMoreResults)
                .Returns(true)
                .Returns(false);

            var mockResponse = new Mock<FeedResponse<FxSpotPriceData>>();
            mockResponse.Setup(r => r.GetEnumerator()).Returns(data.GetEnumerator());

            mockFeedIterator
                .Setup(f => f.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            // Setup for the LINQ queryable with extension method support
            var mockQueryable = new Mock<IOrderedQueryable<FxSpotPriceData>>();

            MockContainer
                .Setup(c => c.GetItemLinqQueryable<FxSpotPriceData>(
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<QueryRequestOptions>(),
                    It.IsAny<CosmosLinqSerializerOptions>()))
                .Returns(mockQueryable.Object);

            // Mock the extension method ToFeedIterator
            // Since we can't directly mock extension methods, we'll intercept the query execution
            var mockQueryProvider = new Mock<IQueryProvider>();
            mockQueryProvider
                .Setup(p => p.Execute(It.IsAny<System.Linq.Expressions.Expression>()))
                .Returns(data.AsQueryable());

            mockQueryable.As<IQueryable<FxSpotPriceData>>()
                .Setup(q => q.Provider)
                .Returns(mockQueryProvider.Object);

            mockQueryable.As<IQueryable<FxSpotPriceData>>()
                .Setup(q => q.Expression)
                .Returns(data.AsQueryable().Expression);

            mockQueryable.As<IQueryable<FxSpotPriceData>>()
                .Setup(q => q.ElementType)
                .Returns(data.AsQueryable().ElementType);

            mockQueryable.As<IQueryable<FxSpotPriceData>>()
                .Setup(q => q.GetEnumerator())
                .Returns(data.GetEnumerator());

            // Mock the cosmos extension method
            MockContainer
                .Setup(c => c.GetItemQueryIterator<FxSpotPriceData>(
                    It.IsAny<QueryDefinition>(),
                    It.IsAny<string>(),
                    It.IsAny<QueryRequestOptions>()))
                .Returns(mockFeedIterator.Object);

            // Act
            var result = await Repository.QueryAsync(e => e.AssetId == "eurusd");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetLatestMarketDataAsync_ShouldReturnLatestVersion()
        {
            // Arrange
            var mockFeedIterator = new Mock<FeedIterator<FxSpotPriceData>>();
            mockFeedIterator
                .SetupSequence(f => f.HasMoreResults)
                .Returns(true)
                .Returns(false);

            var mockResponse = new Mock<FeedResponse<FxSpotPriceData>>();
            mockResponse.Setup(r => r.GetEnumerator()).Returns(new List<FxSpotPriceData> { MarketData }.GetEnumerator());

            mockFeedIterator
                .Setup(f => f.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            MockContainer
                .Setup(c => c.GetItemQueryIterator<FxSpotPriceData>(
                    It.IsAny<QueryDefinition>(),
                    It.IsAny<string>(),
                    It.IsAny<QueryRequestOptions>()))
                .Returns(mockFeedIterator.Object);

            // Act
            var result = await Repository.GetLatestMarketDataAsync(
                "price.spot", "fx", "eurusd", "global",
                new DateOnly(2025, 5, 14), "official");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(MarketData.Price, result.Price);
        }

        [Fact]
        public async Task QueryWithPaginationAsync_ShouldHandleMultiplePages()
        {
            // Arrange
            var data1 = new List<FxSpotPriceData>
            {
                MarketData
            };

            var data2 = new List<FxSpotPriceData>
            {
                new FxSpotPriceData
                {
                    AssetId = "gbpusd",
                    Price = 1.3m,
                    SchemaVersion = "0.0.0",
                    AssetClass = "fx",
                    DataType = "price.spot",
                    Region = "global",
                    DocumentType = "official",
                    AsOfDate = new DateOnly(2025, 5, 14)
                }
            };

            var mockFeedIterator = new Mock<FeedIterator<FxSpotPriceData>>();
            mockFeedIterator
                .SetupSequence(f => f.HasMoreResults)
                .Returns(true)
                .Returns(true)
                .Returns(false);

            var mockResponse1 = new Mock<FeedResponse<FxSpotPriceData>>();
            mockResponse1.Setup(r => r.GetEnumerator()).Returns(data1.GetEnumerator());

            var mockResponse2 = new Mock<FeedResponse<FxSpotPriceData>>();
            mockResponse2.Setup(r => r.GetEnumerator()).Returns(data2.GetEnumerator());

            mockFeedIterator
                .SetupSequence(f => f.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse1.Object)
                .ReturnsAsync(mockResponse2.Object);

            // Setup for the LINQ queryable
            var mockQueryable = new Mock<IOrderedQueryable<FxSpotPriceData>>();

            MockContainer
                .Setup(c => c.GetItemLinqQueryable<FxSpotPriceData>(
                    It.IsAny<bool>(),
                    It.IsAny<string>(),
                    It.IsAny<QueryRequestOptions>(),
                    It.IsAny<CosmosLinqSerializerOptions>()))
                .Returns(mockQueryable.Object);

            // Mock the extension method ToFeedIterator by setting up GetItemQueryIterator
            MockContainer
                .Setup(c => c.GetItemQueryIterator<FxSpotPriceData>(
                    It.IsAny<QueryDefinition>(),
                    It.IsAny<string>(),
                    It.IsAny<QueryRequestOptions>()))
                .Returns(mockFeedIterator.Object);

            // Set up mock query provider
            var combinedData = data1.Concat(data2).ToList();
            var mockQueryProvider = new Mock<IQueryProvider>();
            mockQueryProvider
                .Setup(p => p.Execute(It.IsAny<System.Linq.Expressions.Expression>()))
                .Returns(combinedData.AsQueryable());

            mockQueryable.As<IQueryable<FxSpotPriceData>>()
                .Setup(q => q.Provider)
                .Returns(mockQueryProvider.Object);

            mockQueryable.As<IQueryable<FxSpotPriceData>>()
                .Setup(q => q.Expression)
                .Returns(combinedData.AsQueryable().Expression);

            mockQueryable.As<IQueryable<FxSpotPriceData>>()
                .Setup(q => q.ElementType)
                .Returns(combinedData.AsQueryable().ElementType);

            mockQueryable.As<IQueryable<FxSpotPriceData>>()
                .Setup(q => q.GetEnumerator())
                .Returns(combinedData.GetEnumerator());

            // Act
            var result = await Repository.QueryAsync(e => e.AssetClass == "fx");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());  // Items from both pages
            Assert.Contains(result, item => item.AssetId == "eurusd");
            Assert.Contains(result, item => item.AssetId == "gbpusd");
        }
    }
}