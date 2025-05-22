using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Moq;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Infrastructure.Tests.Repository.Base;
using Xunit;

namespace Phoenix.MarketData.Infrastructure.Tests.Repository
{
    public class CrudOperationTests : BaseMarketDataRepositoryTests
    {
        [Fact]
        public async Task GetByIdAsync_ShouldReturnData_WhenItemExists()
        {
            // Arrange
            var mockResponse = new Mock<ItemResponse<FxSpotPriceData>>();
            mockResponse.Setup(r => r.Resource).Returns(MarketData);
            mockResponse.Setup(r => r.StatusCode).Returns(System.Net.HttpStatusCode.OK); // Add this line

            MockContainer
                .Setup(c => c.ReadItemAsync<FxSpotPriceData>(
                    It.Is<string>(v => v == Id),
                    It.Is<PartitionKey>(pk => pk.ToString().Contains(PartitionKey)),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            // Act
            var result = await Repository.GetByIdAsync(Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(MarketData.Price, result.Price);
            Assert.Equal(MarketData.AssetId, result.AssetId);

            MockContainer.Verify(c =>
                c.ReadItemAsync<FxSpotPriceData>(
                    It.Is<string>(v => v == Id),
                    It.Is<PartitionKey>(pk => pk.ToString().Contains(PartitionKey)),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenItemNotFound()
        {
            // Arrange
            var cosmosException = new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 404, "1", 1.0);

            MockContainer
                .Setup(c => c.ReadItemAsync<FxSpotPriceData>(
                    It.IsAny<string>(),
                    It.IsAny<PartitionKey>(),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(cosmosException);

            // Act
            var result = await Repository.GetByIdAsync(Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallUpsertItemAsync()
        {
            // Arrange
            var mockResponse = new Mock<ItemResponse<FxSpotPriceData>>();
            mockResponse.Setup(r => r.Resource).Returns(MarketData);

            MockContainer
                .Setup(c => c.UpsertItemAsync(
                    It.Is<FxSpotPriceData>(m => m.Id == MarketData.Id),
                    It.Is<PartitionKey>(pk => pk.ToString().Contains(PartitionKey)),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            // Act
            var result = await Repository.UpdateAsync(MarketData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(MarketData.Id, result.Id);

            MockContainer.Verify(c =>
                c.UpsertItemAsync(
                    It.Is<FxSpotPriceData>(m => m.Id == MarketData.Id),
                    It.Is<PartitionKey>(pk => pk.ToString().Contains(PartitionKey)),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify event was published
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.Is<string>(topic => topic.Contains("updated")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_ShouldCallCreateItemAsync()
        {
            // Arrange
            var mockResponse = new Mock<ItemResponse<FxSpotPriceData>>();
            mockResponse.Setup(r => r.Resource).Returns(MarketData);

            MockContainer
                .Setup(c => c.CreateItemAsync(
                    It.Is<FxSpotPriceData>(m => m.Id == MarketData.Id),
                    It.Is<PartitionKey>(pk => pk.ToString().Contains(PartitionKey)),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);

            // Act
            var result = await Repository.AddAsync(MarketData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(MarketData.Id, result.Id);

            MockContainer.Verify(c =>
                c.CreateItemAsync(
                    It.Is<FxSpotPriceData>(m => m.Id == MarketData.Id),
                    It.Is<PartitionKey>(pk => pk.ToString().Contains(PartitionKey)),
                    It.IsAny<ItemRequestOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify event was published
            MockEventPublisher.Verify(e =>
                e.PublishAsync(
                    It.IsAny<object>(),
                    It.Is<string>(topic => topic.Contains("created")),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}