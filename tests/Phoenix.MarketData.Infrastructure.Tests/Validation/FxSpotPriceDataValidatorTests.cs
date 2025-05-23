using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Phoenix.MarketData.Domain.Validation;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Validation;
using Phoenix.MarketData.Domain;

namespace Phoenix.MarketData.Infrastructure.Tests.Validation
{
    public class FxSpotPriceDataValidatorTests
    {
        [Fact]
        public async Task ValidateAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var validator = new FxSpotPriceDataValidator();
            var entity = new FxSpotPriceData
            {
                AssetId = "EUR/USD",
                AssetClass = "fx",
                DataType = "spotprice",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                Price = 1.05m,
                Side = PriceSide.Bid,
                SchemaVersion = "1.0",
                Region = "global",
                DocumentType = "price"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WithNullEntity_ThrowsArgumentNullException()
        {
            // Arrange
            var validator = new FxSpotPriceDataValidator();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                validator.ValidateAsync(null!));
        }

        [Fact]
        public async Task ValidateAsync_WithInvalidAssetIdFormat_ReturnsFailure()
        {
            // Arrange
            var validator = new FxSpotPriceDataValidator();
            var entity = new FxSpotPriceData
            {
                AssetId = "EURUSD", // Missing separator
                AssetClass = "fx",
                DataType = "spotprice",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                Price = 1.05m,
                Side = PriceSide.Bid,
                SchemaVersion = "1.0",
                Region = "global",
                DocumentType = "price"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AssetId");
        }

        [Fact]
        public async Task ValidateAsync_WithNegativePrice_ReturnsFailure()
        {
            // Arrange
            var validator = new FxSpotPriceDataValidator();
            var entity = new FxSpotPriceData
            {
                AssetId = "EUR/USD",
                AssetClass = "fx",
                DataType = "spotprice",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                Price = -1.05m, // Negative value
                Side = PriceSide.Mid,
                SchemaVersion = "1.0",
                Region = "global",
                DocumentType = "price"
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Price");
        }
    }
}