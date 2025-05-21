using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Moq;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Infrastructure.Repositories; // <-- Adjust if necessary
using Xunit;

public class MarketDataRepositoryTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly CosmosRepository<FxSpotPriceData> _repository;
    private readonly string _id;
    private readonly string _partitionKey;
    private readonly FxSpotPriceData _marketData;

    public MarketDataRepositoryTests()
    {
        _mockContainer = new Mock<Container>();
        _repository = new CosmosRepository<FxSpotPriceData>(_mockContainer.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<CosmosRepository<FxSpotPriceData>>>());

        _id = "price.spot__fx__eurusd__global__2025-05-14__official__1";
        _partitionKey = "eurusd";
        _marketData = new FxSpotPriceData
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
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnData_WhenItemExists()
    {
        // Arrange
        var mockResponse = new Mock<ItemResponse<FxSpotPriceData>>();
        mockResponse.Setup(r => r.Resource).Returns(_marketData);

        _mockContainer
            .Setup(c => c.ReadItemAsync<FxSpotPriceData>(
                It.Is<string>(v => v == _id),
                It.Is<PartitionKey>(pk => pk.ToString().Contains(_partitionKey)), // Loose check for demo
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _repository.GetByIdAsync(_id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_marketData.Price, result.Price);
        Assert.Equal(_marketData.AssetId, result.AssetId);
    }

    [Fact]
    public async Task SaveMarketDataAsync_ShouldCallUpsertItemAsync()
    {
        var mockResponse = new Mock<ItemResponse<FxSpotPriceData>>();
        mockResponse.Setup(r => r.Resource).Returns(_marketData);

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.Is<FxSpotPriceData>(m => m.Id == _marketData.Id),
                It.Is<PartitionKey>(pk => pk.ToString().Contains(_partitionKey)),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _repository.UpdateAsync(_marketData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_marketData.Id, result.Id);

        _mockContainer.Verify(c =>
            c.UpsertItemAsync(
                It.Is<FxSpotPriceData>(m => m.Id == _marketData.Id),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
