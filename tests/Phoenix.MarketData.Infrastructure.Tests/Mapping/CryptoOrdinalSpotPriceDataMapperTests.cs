using System;
using System.Collections.Generic;
using Xunit;
using Phoenix.MarketData.Domain; // Add this for PriceSide enum
using Phoenix.MarketData.Domain.Models;
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
                Currency = "USD"
            };

            // Use SetTags method to initialize Tags collection
            domain.SetTags(new List<string> { "tag1", "tag2" });

            // Act
            var result = mapper.ToDto(domain);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(domain.DisplayAssetId, result.AssetId); // Check against DisplayAssetId
            Assert.Equal(domain.AsOfDate, result.AsOfDate);
            Assert.Equal(domain.AsOfTime, result.AsOfTime);
            Assert.Equal(domain.Price, result.Price);
            Assert.Equal((PriceSideDto)domain.Side, result.Side); // Fix: Cast to PriceSideDto
            Assert.Equal(domain.CollectionName, result.CollectionName);
            Assert.Equal(domain.ParentInscriptionId, result.ParentInscriptionId);
            Assert.Equal(domain.InscriptionId, result.InscriptionId);

            // Check tags were mapped correctly
            Assert.NotNull(result.Tags);
            Assert.Equal(2, result.Tags.Count);
            Assert.Contains("tag1", result.Tags);
            Assert.Contains("tag2", result.Tags);
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
                side: PriceSide.Mid,
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
            Assert.Equal(PriceSide.Mid, result.Side); // Check PriceSide conversion 
            Assert.Equal(dto.CollectionName, result.CollectionName);
            Assert.Equal(dto.ParentInscriptionId, result.ParentInscriptionId);
            Assert.Equal(dto.InscriptionId, result.InscriptionId);

            // Check tags were mapped correctly
            Assert.NotNull(result.Tags);
            Assert.Equal(2, result.Tags.Count);
            Assert.Contains("tag1", result.Tags);
            Assert.Contains("tag2", result.Tags);
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
                AsOfTime = TimeOnly.FromDateTime(DateTime.Now),  // Added missing property
                // Add required properties
                SchemaVersion = "1.0",
                AssetClass = "crypto",
                DataType = "spotprice",
                Region = "global",
                DocumentType = "price"
            };
            domain.SetTags(new List<string>());  // Use the appropriate method to set tags

            var instanceMapper = new CryptoOrdinalSpotPriceDataMapper();

            // Act
            var instanceResult = instanceMapper.ToDto(domain);
            var staticResult = CryptoOrdinalSpotPriceDataMapper.MapToDto(domain);

            // Assert
            Assert.Equal(instanceResult.AssetId, staticResult.AssetId);
            Assert.Equal(instanceResult.Price, staticResult.Price);
            Assert.Equal(instanceResult.CollectionName, staticResult.CollectionName);
        }

        [Theory]
        [InlineData(PriceSide.Ask)]
        [InlineData(PriceSide.Bid)]
        [InlineData(PriceSide.Mid)]
        public void PriceSideConversion_ShouldMapCorrectly_BothDirections(PriceSide side)
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();
            var domain = new CryptoOrdinalSpotPriceData
            {
                AssetId = "BTC/USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),  // Fixed missing required property
                AsOfTime = TimeOnly.FromDateTime(DateTime.Now),    // Added for completeness
                Price = 50000.00m,
                Side = side,  // Use the provided enum value
                SchemaVersion = "1.0",
                AssetClass = "crypto",
                DataType = "spotprice",
                Region = "global",
                DocumentType = "price",
                CollectionName = "Bitcoin",           // Fixed missing required property
                ParentInscriptionId = "12345",        // Fixed missing required property
                InscriptionId = "67890",              // Fixed missing required property
                Currency = "USD"                      // Fixed missing required property
            };
            domain.SetTags(new List<string>());  // Initialize tags

            // Act - Convert Domain to DTO
            var dto = mapper.ToDto(domain);

            // Assert - DTO should have correct side
            Assert.Equal((PriceSideDto)side, dto.Side);

            // Act - Convert DTO back to Domain
            var convertedDomain = mapper.ToDomain(dto);

            // Assert - Original side value should be preserved
            Assert.Equal(side, convertedDomain.Side);
        }

        [Theory]
        [InlineData(PriceSide.Ask, PriceSide.Ask)]  // Fixed: Changed from PriceSideDto to PriceSide
        [InlineData(PriceSide.Bid, PriceSide.Bid)]  // Fixed: Changed from PriceSideDto to PriceSide
        [InlineData(PriceSide.Mid, PriceSide.Mid)]  // Fixed: Changed from PriceSideDto to PriceSide
        public void PriceSideDtoToDomainConversion_ShouldMapCorrectly(PriceSide dtoSide, PriceSide expectedDomainSide)
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();
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
                tags: new List<string>(),
                price: 50000.00m,
                side: dtoSide,  // Fixed: Changed parameter type from PriceSideDto to PriceSide
                collectionName: "Bitcoin",
                parentInscriptionId: "12345",
                inscriptionId: "67890",
                inscriptionNumber: 123,
                currency: "USD"
            );

            // Act
            var result = mapper.ToDomain(dto);

            // Assert
            Assert.Equal(expectedDomainSide, result.Side);
        }

        [Fact]
        public void ToDomain_WithNullPriceSideDto_DefaultsToMid()
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();
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
                tags: new List<string>(),
                price: 50000.00m,
                side: null,  // Null side
                collectionName: "Bitcoin",
                parentInscriptionId: "12345",
                inscriptionId: "67890",
                inscriptionNumber: 123,
                currency: "USD"
            );

            // Act
            var result = mapper.ToDomain(dto);

            // Assert - should default to Mid
            Assert.Equal(PriceSide.Mid, result.Side);
        }

        [Theory]
        [InlineData(PriceSide.Bid)]
        [InlineData(PriceSide.Ask)]
        [InlineData(PriceSide.Mid)]
        public void ToDto_MapsAllPriceSideEnumValues(PriceSide side)
        {
            // Arrange
            var mapper = new CryptoOrdinalSpotPriceDataMapper();
            var domain = new CryptoOrdinalSpotPriceData
            {
                AssetId = "BTC/USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                AsOfTime = TimeOnly.FromDateTime(DateTime.Now),
                Price = 50000.00m,
                Side = side,
                SchemaVersion = "1.0",
                AssetClass = "crypto",
                DataType = "spotprice",
                Region = "global",
                DocumentType = "price",
                CollectionName = "Bitcoin",
                ParentInscriptionId = "12345",
                InscriptionId = "67890",
                InscriptionNumber = 123,
                Currency = "USD"
            };
            domain.SetTags(new List<string> { "tag1", "tag2" });

            // Act
            var dto = mapper.ToDto(domain);

            // Assert
            Assert.Equal((PriceSideDto)side, dto.Side);
        }
    }
}