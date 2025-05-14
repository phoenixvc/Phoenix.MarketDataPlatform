using Microsoft.Azure.Cosmos;
using Moq;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Cosmos;

public class MarketDataRepositoryTests
{
    private readonly Mock<Container> _mockContainer;
    private readonly MarketDataRepository _repository;
    private string _id;
    private string _partitionKey;
    private FxSpotPriceData _marketData;

    public MarketDataRepositoryTests()
    {
        // Arrange mock for Cosmos Container
        _mockContainer = new Mock<Container>();
        _repository = new MarketDataRepository(_mockContainer.Object);
        
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
            Region = Phoenix.MarketData.Domain.Regions.Global,
            DocumentType = "official",
            AsOfDate = new DateOnly(2025, 5, 14)
        };
    }
    
    [Fact]
    public async Task GetMarketDataAsync_ShouldReturnData_WhenItemExists()
    {
        // Arrange
        var mockResponse = new Mock<ItemResponse<FxSpotPriceData>>();
        mockResponse.Setup(r => r.Resource).Returns(_marketData);

        _mockContainer
            .Setup(c => c.ReadItemAsync<FxSpotPriceData>(
                It.Is<string>(value => value == _id),
                It.Is<PartitionKey>(pk => pk.ToString() == _partitionKey),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _repository.GetMarketDataBySpecifiedVersionAsync<FxSpotPriceData>("price.spot",
            "fx", "eurusd", Phoenix.MarketData.Domain.Regions.Global,
            new DateOnly(2025, 5, 14), "official", 1);

        // Assert
        //Assert.NotNull(result);
        //Assert.Equal("Test Data", result.Data);
        //Assert.Equal(id, result.Id);
    }

    // [Fact]
    // public async Task GetMarketDataAsync_ShouldReturnNull_WhenItemDoesNotExist()
    // {
    //     // Arrange
    //     var id = "non-existent-id";
    //     var partitionKey = "partition";
    //
    //     _mockContainer
    //         .Setup(c => c.ReadItemAsync<MarketData>(
    //             It.IsAny<string>(),
    //             It.IsAny<PartitionKey>(),
    //             It.IsAny<ItemRequestOptions>(),
    //             It.IsAny<CancellationToken>()))
    //         .ThrowsAsync(new CosmosException("Not Found", HttpStatusCode.NotFound, 0, "", 0));
    //
    //     // Act
    //     var result = await _repository.GetMarketDataAsync(id, partitionKey);
    //
    //     // Assert
    //     Assert.Null(result);
    // }
    //
    
    [Fact]
    public async Task SaveMarketDataAsync_ShouldCallUpsertItemAsync()
    {
        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.Is<FxSpotPriceData>(m => m.Id == _marketData.Id),
                It.Is<PartitionKey>(pk => pk.ToString() == _partitionKey),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Mock<ItemResponse<FxSpotPriceData>>().Object);
    
        // Act
        var result = await _repository.SaveMarketDataAsync(_marketData);
    
        // Assert
        _mockContainer.Verify(c =>
            c.UpsertItemAsync(
                It.Is<FxSpotPriceData>(m => m.Id == _marketData.Id),
                It.Is<PartitionKey>(pk => pk.ToString() == _partitionKey),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}