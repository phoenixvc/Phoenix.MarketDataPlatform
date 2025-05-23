using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Moq;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Tests.Repository.Base;
using Xunit;

namespace Phoenix.MarketData.Infrastructure.Tests.Repository
{
    public class ErrorHandlingTests : BaseMarketDataRepositoryTests
    {
        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenCosmosThrowsException()
        {
            // Arrange
            var cosmosException = new CosmosException("Failed to create", System.Net.HttpStatusCode.InternalServerError, 500, "1", 1.0);

            MockContainer
                .Setup(c => c.CreateItemAsync(
                    It.IsAny<FxSpotPriceData>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(cosmosException);

            // Act & Assert
            await Assert.ThrowsAsync<CosmosException>(() => Repository.AddAsync(MarketData));

            // Verify no event was published due to failure
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenCosmosThrowsException()
        {
            // Arrange
            var cosmosException = new CosmosException("Failed to update", System.Net.HttpStatusCode.InternalServerError, 500, "1", 1.0);

            MockContainer
                .Setup(c => c.UpsertItemAsync(
                    It.IsAny<FxSpotPriceData>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(cosmosException);

            // Act & Assert
            await Assert.ThrowsAsync<CosmosException>(() => Repository.UpdateAsync(MarketData));

            // Verify no event was published due to failure
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnFalse_WhenItemNotFoundDuringHardDelete()
        {
            // Arrange
            var cosmosException = new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 404, "1", 1.0);

            MockContainer
                .Setup(c => c.DeleteItemAsync<FxSpotPriceData>(
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(cosmosException);

            // Setup for GetPartitionKeyForIdAsync which is called internally
            var mockFeedIterator = new Mock<FeedIterator<FxSpotPriceData>>();
            mockFeedIterator
                .SetupSequence(f => f.HasMoreResults)
                .Returns(true)
                .Returns(false);

            var mockResponse = new Mock<FeedResponse<FxSpotPriceData>>();
            mockResponse.Setup(r => r.Count).Returns(1);

            // Fix: Use a List<FxSpotPriceData> to ensure the correct generic IEnumerator type is returned
            var items = new List<FxSpotPriceData> { MarketData };
            mockResponse.Setup(r => r.GetEnumerator()).Returns(items.GetEnumerator());

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
            var result = await Repository.DeleteAsync(Id, false);

            // Assert
            Assert.False(result);

            // Verify no event was published
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}