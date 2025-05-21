using System;
using System.Collections.Generic;
using Xunit;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Mapping;

namespace Phoenix.MarketData.Infrastructure.Tests.Mapping
{
    public class CryptoOrdinalSpotPriceDataMapperTests
    {
        [Fact]
        public void ToDomain_WithValidData_ReturnsMappedEntity()
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();
            var data = new CryptoOrdinalSpotPriceData
            {
                AssetId = "BTC/USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                AsOfTime = TimeOnly.FromDateTime(DateTime.Now),
                Price = 50000.00m,
                Volume = 100.5m,
                // Set other required properties
            };

            // Act
            var result = mapper.ToDomain(data);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data.AssetId, result.AssetId);
            Assert.Equal(data.AsOfDate, result.AsOfDate);
            Assert.Equal(data.AsOfTime, result.AsOfTime);
            Assert.Equal(data.Price, result.Price);
            Assert.Equal(data.Volume, result.Volume);
            // Assert other mapped properties
        }

        [Fact]
        public void ToDomain_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mapper.ToDomain(null));
        }

        [Theory]
        [InlineData(null, "Missing asset ID")]
        [InlineData("", "Asset ID cannot be empty")]
        public void ToDomain_WithInvalidAssetId_ThrowsException(string assetId, string expectedMessage)
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();
            var data = new CryptoOrdinalSpotPriceData
            {
                AssetId = assetId,
                // Set other required properties
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => mapper.ToDomain(data));
            Assert.Contains(expectedMessage, exception.Message);
        }

        [Fact]
        public void ToDomain_WithNegativePrice_ThrowsException()
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();
            var data = new CryptoOrdinalSpotPriceData
            {
                AssetId = "BTC/USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                Price = -1000.00m,
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => mapper.ToDomain(data));
        }

        [Fact]
        public void ToDomain_WithOptionalFieldsNull_MapsCorrectly()
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();
            var data = new CryptoOrdinalSpotPriceData
            {
                AssetId = "BTC/USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                Price = 50000.00m,
                // Other fields are null
            };

            // Act
            var result = mapper.ToDomain(data);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data.Price, result.Price);
            // Assert default values for optional fields
        }
    }
}