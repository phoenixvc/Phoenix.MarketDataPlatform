using System;
using System.Collections.Generic;
using Xunit;
using Phoenix.MarketData.Core; // Add this for PriceSide enum
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Mapping;
using Phoenix.MarketData.Infrastructure.Serialization;

namespace Phoenix.MarketData.Infrastructure.Tests.Mapping
{
    public class CryptoOrdinalSpotPriceDataMapperTests
    {
        [Fact]
        public void ToDto_WithValidDomain_ReturnsMappedDto()
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();
            var domain = new CryptoOrdinalSpotPriceData
            {
                AssetId = "BTC/USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                AsOfTime = TimeOnly.FromDateTime(DateTime.Now),
                Price = 50000.00m,
                Side = PriceSide.Mid,
                SchemaVersion = "1.0",
                AssetClass = "crypto",
                DataType = "spotprice",
                Region = "global",
                DocumentType = "price",
                CollectionName = "Bitcoin",
                ParentInscriptionId = "12345",
                InscriptionId = "67890",
                InscriptionNumber = 123,
                Currency = "USD",
                Tags = new List<string> { "tag1", "tag2" }
            };

            // Act
            var result = mapper.ToDto(domain);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(domain.DisplayAssetId, result.AssetId); // Check against DisplayAssetId
            Assert.Equal(domain.AsOfDate, result.AsOfDate);
            Assert.Equal(domain.AsOfTime, result.AsOfTime);
            Assert.Equal(domain.Price, result.Price);
            Assert.Equal(domain.CollectionName, result.CollectionName);
            Assert.Equal(domain.ParentInscriptionId, result.ParentInscriptionId);
            Assert.Equal(domain.InscriptionId, result.InscriptionId);
        }

        [Fact]
        public void ToDto_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mapper.ToDto(null!));
        }

        [Fact]
        public void ToDomain_WithValidDto_ReturnsMappedEntity()
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();
            var dto = new CryptoOrdinalSpotPriceDataDto(
                id: "test-id",
                schemaVersion: "1.0",
                version: 1,
                assetId: "BTC/USD", // This should preserve its case
                assetClass: "crypto",
                dataType: "spotprice",
                region: "global",
                documentType: "price",
                createTimestamp: DateTime.UtcNow,
                asOfDate: DateOnly.FromDateTime(DateTime.Today),
                asOfTime: TimeOnly.FromDateTime(DateTime.Now),
                tags: new List<string> { "tag1", "tag2" },
                price: 50000.00m,
                side: PriceSide.Mid,  // Use domain enum (PriceSide) as constructor expects
                collectionName: "Bitcoin",
                parentInscriptionId: "12345",
                inscriptionId: "67890",
                inscriptionNumber: 123,
                currency: "USD"
            );

            // Act
            var result = mapper.ToDomain(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.AssetId, result.DisplayAssetId); // Check DisplayAssetId for original case
            Assert.Equal(dto.AssetId.ToLowerInvariant(), result.AssetId); // Check AssetId for normalized case
            Assert.Equal(dto.AsOfDate, result.AsOfDate);
            Assert.Equal(dto.AsOfTime, result.AsOfTime);
            Assert.Equal(dto.Price, result.Price);
            Assert.Equal(dto.CollectionName, result.CollectionName);
            Assert.Equal(dto.ParentInscriptionId, result.ParentInscriptionId);
            Assert.Equal(dto.InscriptionId, result.InscriptionId);
        }

        [Fact]
        public void ToDomain_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mapper.ToDomain(null!));
        }

        [Fact]
        public void StaticMapToDto_ShouldWorkSameAsInstanceMethod()
        {
            // Arrange
            var domain = new CryptoOrdinalSpotPriceData
            {
                AssetId = "BTC/USD",
                Price = 50000.00m,
                Side = PriceSide.Mid,
                CollectionName = "Bitcoin",
                ParentInscriptionId = "12345",
                InscriptionId = "67890",
                Currency = "USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                // Add required properties
                SchemaVersion = "1.0",
                AssetClass = "crypto",
                DataType = "spotprice",
                Region = "global",
                DocumentType = "price"
            };

            var instanceMapper = new CryptoOrdinalSpotPriceDataMapper();

            // Act
            var instanceResult = instanceMapper.ToDto(domain);
            var staticResult = CryptoOrdinalSpotPriceDataMapper.MapToDto(domain);

            // Assert
            Assert.Equal(instanceResult.AssetId, staticResult.AssetId);
            Assert.Equal(instanceResult.Price, staticResult.Price);
            Assert.Equal(instanceResult.CollectionName, staticResult.CollectionName);
        }

        [Fact]
        public void StaticMapToDomain_ShouldWorkSameAsInstanceMethod()
        {
            // Arrange
            var dto = new CryptoOrdinalSpotPriceDataDto(
                id: "test-id",
                schemaVersion: "1.0",
                version: 1,
                assetId: "BTC/USD",
                assetClass: "crypto",
                dataType: "spotprice",
                region: "global",
                documentType: "price",
                createTimestamp: DateTime.UtcNow,
                asOfDate: DateOnly.FromDateTime(DateTime.Today),
                asOfTime: TimeOnly.FromDateTime(DateTime.Now),
                tags: new List<string> { "tag1", "tag2" },
                price: 50000.00m,
                side: PriceSide.Mid,  // Use domain enum (PriceSide) as constructor expects
                collectionName: "Bitcoin",
                parentInscriptionId: "12345",
                inscriptionId: "67890",
                inscriptionNumber: 123,
                currency: "USD"
            );

            var instanceMapper = new CryptoOrdinalSpotPriceDataMapper();

            // Act
            var instanceResult = instanceMapper.ToDomain(dto);
            var staticResult = CryptoOrdinalSpotPriceDataMapper.MapToDomain(dto);

            // Assert
            Assert.Equal(instanceResult.AssetId, staticResult.AssetId);
            Assert.Equal(instanceResult.Price, staticResult.Price);
            Assert.Equal(instanceResult.CollectionName, staticResult.CollectionName);
        }
    }
}