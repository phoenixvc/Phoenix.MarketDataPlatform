using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Moq;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Repositories;
using Phoenix.MarketData.Infrastructure.Tests.Repository.Base;
using Xunit;

namespace Phoenix.MarketData.Infrastructure.Tests.Repository
{
    public class BatchOperationTests : BaseMarketDataRepositoryTests
    {
        [Fact]
        public async Task AddBatchAsync_ShouldCallCreateItemAsyncForEachItem()
        {
            // Arrange
            var secondItem = new FxSpotPriceData
            {
                Price = 1.2m,
                AssetId = "jpyusd",
                AssetClass = "fx",
                DataType = "price.spot",
                Region = "global",
                DocumentType = "official",
                AsOfDate = new DateOnly(2025, 5, 14),
                SchemaVersion = "0.0.0"
                // Don't set Id directly, it's calculated from the above properties
            };

            var items = new List<FxSpotPriceData> { MarketData, secondItem };
            var mockResponse = new Mock<ItemResponse<FxSpotPriceData>>();
            mockResponse.Setup(r => r.Resource).Returns(MarketData);

            MockContainer
                .Setup(c => c.CreateItemAsync(
                    It.IsAny<FxSpotPriceData>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            // Act
            await Repository.AddSequentialAsync(items);

            // Assert
            MockContainer.Verify(c =>
                c.CreateItemAsync(
                    It.IsAny<FxSpotPriceData>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            // Verify events were published for each item
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.Is<string>(topic => topic.Contains("created")),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateBatchAsync_ShouldCallUpsertItemAsyncForEachItem()
        {
            // Arrange
            var secondItem = new FxSpotPriceData
            {
                Price = 1.2m,
                AssetId = "jpyusd",
                AssetClass = "fx",
                DataType = "price.spot",
                Region = "global",
                DocumentType = "official",
                AsOfDate = new DateOnly(2025, 5, 14),
                SchemaVersion = "0.0.0"
                // Don't set Id directly, it's calculated from the above properties
            };

            var items = new List<FxSpotPriceData> { MarketData, secondItem };
            var mockResponse = new Mock<ItemResponse<FxSpotPriceData>>();
            mockResponse.Setup(r => r.Resource).Returns(MarketData);

            MockContainer
                .Setup(c => c.UpsertItemAsync(
                    It.IsAny<FxSpotPriceData>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            // Act
            await Repository.UpdateSequentialAsync(items);

            // Assert
            MockContainer.Verify(c =>
                c.UpsertItemAsync(
                    It.IsAny<FxSpotPriceData>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            // Verify events were published for each item
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.Is<string>(topic => topic.Contains("updated")),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task DeleteBatchAsync_ShouldCallDeleteItemAsyncForEachItem()
        {
            // Arrange
            var ids = new List<string> { Id, "price.spot__fx__jpyusd__global__2025-05-14__official__1" };

            MockContainer
                .Setup(c => c.DeleteItemAsync<FxSpotPriceData>(
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ItemResponse<FxSpotPriceData>>().Object);

            // Setup for GetPartitionKeyForIdAsync which is called internally
            var mockFeedIterator = new Mock<FeedIterator<FxSpotPriceData>>();
            mockFeedIterator
                .SetupSequence(f => f.HasMoreResults)
                .Returns(true)
                .Returns(false);

            var mockResponse = new Mock<FeedResponse<FxSpotPriceData>>();
            mockResponse.Setup(r => r.Count).Returns(1);

            // Fix IEnumerator issue by returning the generic IEnumerator<FxSpotPriceData>
            mockResponse
                .Setup(r => r.GetEnumerator())
                .Returns(new List<FxSpotPriceData> { MarketData }.GetEnumerator());

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
            await Repository.DeleteSequentialAsync(ids, false);

            // Assert
            MockContainer.Verify(c =>
                c.DeleteItemAsync<FxSpotPriceData>(
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            // Verify events were published for each deletion
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.Is<string>(topic => topic.Contains("deleted")),
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
    }
}