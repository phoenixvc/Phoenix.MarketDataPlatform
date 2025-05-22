using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Moq;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Infrastructure.Tests.Repository.Base;
using Xunit;

namespace Phoenix.MarketData.Infrastructure.Tests.Repository
{
    public class DeleteOperationTests : BaseMarketDataRepositoryTests
    {
        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteItemAsync_WhenUsingHardDelete()
        {
            // Arrange
            MockContainer
                .Setup(c => c.DeleteItemAsync<FxSpotPriceData>(
                    It.Is<string>(id => id == Id),
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

            // Fix the IEnumerator issue by using a List<T> instead of an array
            mockResponse.Setup(r => r.GetEnumerator())
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
            var result = await Repository.DeleteAsync(Id, false);

            // Assert
            Assert.True(result);

            MockContainer.Verify(c =>
                c.DeleteItemAsync<FxSpotPriceData>(
                    It.Is<string>(id => id == Id),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify event was published
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.Is<string>(topic => topic.Contains("deleted")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}