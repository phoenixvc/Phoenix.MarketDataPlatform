using System;
using System.Collections.Generic;
using Xunit;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Mapping;

namespace Phoenix.MarketData.Infrastructure.Tests.Mapping
{
    public class FxSpotPriceDataMapperTests
    {
        [Fact]
        public void ToDomain_WithValidData_ReturnsMappedEntity()
        {
            // Arrange
            var mapper = new FxSpotPriceDataMapper();
            var data = new FxSpotPriceData
            {
                AssetId = "EUR/USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                AsOfTime = TimeOnly.FromDateTime(DateTime.Now),
                Bid = 1.05m,
                Ask = 1.06m,
                // Set other required properties
            };

            // Act
            var result = mapper.ToDomain(data);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(data.AssetId, result.AssetId);
            Assert.Equal(data.AsOfDate, result.AsOfDate);
            Assert.Equal(data.AsOfTime, result.AsOfTime);
            Assert.Equal(data.Bid, result.Bid);
            Assert.Equal(data.Ask, result.Ask);
            // Assert other mapped properties
        }

        [Fact]
        public void ToDomain_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            var mapper = new FxSpotPriceDataMapper();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mapper.ToDomain(null));
        }

        [Theory]
        [InlineData(null, "Missing asset ID")]
        [InlineData("", "Asset ID cannot be empty")]
        public void ToDomain_WithInvalidAssetId_ThrowsException(string assetId, string expectedMessage)
        {
            // Arrange
            var mapper = new FxSpotPriceDataMapper();
            var data = new FxSpotPriceData
            {
                AssetId = assetId,
                // Set other required properties
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => mapper.ToDomain(data));
            Assert.Contains(expectedMessage, exception.Message);
        }

        [Fact]
        public void ToDomain_WithMissingBidAsk_CalculatesMidAsNull()
        {
            // Arrange
            var mapper = new FxSpotPriceDataMapper();
            var data = new FxSpotPriceData
            {
                AssetId = "EUR/USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                // Missing Bid/Ask
            };

            // Act
            var result = mapper.ToDomain(data);

            // Assert
            Assert.Null(result.Mid);
        }

        [Fact]
        public void ToDomain_WithValidBidAsk_CalculatesMidCorrectly()
        {
            // Arrange
            var mapper = new FxSpotPriceDataMapper();
            var data = new FxSpotPriceData
            {
                AssetId = "EUR/USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                Bid = 1.05m,
                Ask = 1.07m,
            };

            // Act
            var result = mapper.ToDomain(data);

            // Assert
            Assert.Equal(1.06m, result.Mid);
        }
    }
}