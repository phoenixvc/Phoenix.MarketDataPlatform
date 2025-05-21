using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Validation;
using Phoenix.MarketData.Core.Models;

namespace Phoenix.MarketData.Infrastructure.Tests.Validation
{
    public class CryptoOrdinalSpotPriceDataValidatorTests
    {
        [Fact]
        public async Task ValidateAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var validator = new CryptoOrdinalSpotPriceDataValidator();
            var entity = new CryptoOrdinalSpotPriceData
            {
                AssetId = "BTC/USD",
                AssetClass = "crypto",
                DataType = "spotprice",
                SchemaVersion = "1.0",
                Region = "global",
                DocumentType = "price",
                Currency = "USD",
                CollectionName = "Bitcoin",
                ParentInscriptionId = "12345",
                InscriptionId = "67890",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                Price = 50000.00m
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
            var validator = new CryptoOrdinalSpotPriceDataValidator();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                validator.ValidateAsync(null!));
        }

        [Fact]
        public async Task ValidateAsync_WithMissingAssetId_ReturnsFailure()
        {
            // Arrange
            var validator = new CryptoOrdinalSpotPriceDataValidator();
            var entity = new CryptoOrdinalSpotPriceData
            {
                // AssetId is missing/empty
                AssetId = string.Empty, // Using empty string instead of null for non-nullable
                AssetClass = "crypto",
                DataType = "spotprice",
                SchemaVersion = "1.0",
                Region = "global",
                DocumentType = "price",
                Currency = "USD",
                CollectionName = "Bitcoin",
                ParentInscriptionId = "12345",
                InscriptionId = "67890",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                Price = 50000.00m
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AssetId");
        }

        [Fact]
        public async Task ValidateAsync_WithInvalidAssetClass_ReturnsFailure()
        {
            // Arrange
            var validator = new CryptoOrdinalSpotPriceDataValidator();
            var entity = new CryptoOrdinalSpotPriceData
            {
                AssetId = "BTC/USD",
                AssetClass = "invalid",  // Should be "crypto"
                DataType = "spotprice",
                SchemaVersion = "1.0",
                Region = "global",
                DocumentType = "price",
                Currency = "USD",
                CollectionName = "Bitcoin",
                ParentInscriptionId = "12345",
                InscriptionId = "67890",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                Price = 50000.00m
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AssetClass");
        }

        [Fact]
        public async Task ValidateAsync_WithNegativePrice_ReturnsFailure()
        {
            // Arrange
            var validator = new CryptoOrdinalSpotPriceDataValidator();
            var entity = new CryptoOrdinalSpotPriceData
            {
                AssetId = "BTC/USD",
                AssetClass = "crypto",
                DataType = "spotprice",
                SchemaVersion = "1.0",
                Region = "global",
                DocumentType = "price",
                Currency = "USD",
                CollectionName = "Bitcoin",
                ParentInscriptionId = "12345",
                InscriptionId = "67890",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                Price = -500.00m  // Negative price
            };

            // Act
            var result = await validator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Price");
        }
    }
}