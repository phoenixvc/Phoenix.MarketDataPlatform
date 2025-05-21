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
    public class FxSpotPriceDataMapperTests
    {
        [Fact]
        public void ToDto_WithValidDomain_ReturnsMappedDto()
        {
            // Arrange
            var mapper = new FxSpotPriceDataMapper();
            var domain = new FxSpotPriceData
            {
                AssetId = "EUR/USD",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                AsOfTime = TimeOnly.FromDateTime(DateTime.Now),
                Price = 1.05m,
                Side = PriceSide.Bid,
                SchemaVersion = "1.0",
                AssetClass = "fx",
                DataType = "spotprice",
                Region = "global",
                DocumentType = "price",
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
            Assert.Equal(PriceSideDto.Bid, result.Side); // Check the DTO enum type
        }

        [Fact]
        public void ToDto_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            var mapper = new FxSpotPriceDataMapper();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mapper.ToDto(null!));
        }

        [Fact]
        public void ToDomain_WithValidDto_ReturnsMappedEntity()
        {
            // Arrange
            var mapper = new FxSpotPriceDataMapper();
            var dto = new FxSpotPriceDataDto(
                id: "test-id",
                schemaVersion: "1.0",
                version: 1,
                assetId: "EUR/USD",
                assetClass: "fx",
                dataType: "spotprice",
                region: "global",
                documentType: "price",
                createTimeStamp: DateTime.UtcNow,  // Note capital 'S'
                asOfDate: DateOnly.FromDateTime(DateTime.Today),
                asOfTime: TimeOnly.FromDateTime(DateTime.Now),
                tags: new List<string> { "tag1", "tag2" },
                price: 1.05m,
                side: PriceSide.Bid  // Use domain enum (PriceSide) as constructor expects
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
            Assert.Equal(PriceSide.Bid, result.Side); // Check domain enum type
        }

        [Fact]
        public void ToDomain_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            var mapper = new FxSpotPriceDataMapper();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mapper.ToDomain(null!));
        }

        [Fact]
        public void StaticMapToDto_ShouldWorkSameAsInstanceMethod()
        {
            // Arrange
            var domain = new FxSpotPriceData
            {
                AssetId = "EUR/USD",
                Price = 1.05m,
                Side = PriceSide.Bid,
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                // Add required properties
                SchemaVersion = "1.0",
                AssetClass = "fx",
                DataType = "spotprice",
                Region = "global",
                DocumentType = "price"
            };

            var instanceMapper = new FxSpotPriceDataMapper();

            // Act
            var instanceResult = instanceMapper.ToDto(domain);
            var staticResult = FxSpotPriceDataMapper.MapToDto(domain);

            // Assert
            Assert.Equal(instanceResult.AssetId, staticResult.AssetId);
            Assert.Equal(instanceResult.Price, staticResult.Price);
            Assert.Equal(instanceResult.Side, staticResult.Side);
        }

        [Fact]
        public void StaticMapToDomain_ShouldWorkSameAsInstanceMethod()
        {
            // Arrange
            var dto = new FxSpotPriceDataDto(
                id: "test-id",
                schemaVersion: "1.0",
                version: 1,
                assetId: "EUR/USD",
                assetClass: "fx",
                dataType: "spotprice",
                region: "global",
                documentType: "price",
                createTimeStamp: DateTime.UtcNow,  // Note capital 'S'
                asOfDate: DateOnly.FromDateTime(DateTime.Today),
                asOfTime: TimeOnly.FromDateTime(DateTime.Now),
                tags: new List<string> { "tag1", "tag2" },
                price: 1.05m,
                side: PriceSide.Bid  // Use domain enum (PriceSide) as constructor expects
            );

            var instanceMapper = new FxSpotPriceDataMapper();

            // Act
            var instanceResult = instanceMapper.ToDomain(dto);
            var staticResult = FxSpotPriceDataMapper.MapToDomain(dto);

            // Assert
            Assert.Equal(instanceResult.AssetId, staticResult.AssetId);
            Assert.Equal(instanceResult.Price, staticResult.Price);
            Assert.Equal(instanceResult.Side, staticResult.Side);
        }
    }
}
