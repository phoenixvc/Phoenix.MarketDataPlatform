using Phoenix.MarketData.Core;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Serialization;
using System;
using System.Collections.Generic;

namespace Phoenix.MarketData.Infrastructure.Mapping
{
    public class FxSpotPriceDataMapper : IMarketDataMapper<FxSpotPriceData, FxSpotPriceDataDto>
    {
        public FxSpotPriceDataDto ToDto(FxSpotPriceData domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            var dto = new FxSpotPriceDataDto(
                id: domain.Id,
                schemaVersion: domain.SchemaVersion,
                version: domain.Version,
                assetId: domain.DisplayAssetId, // Use DisplayAssetId to preserve original case
                assetClass: domain.AssetClass,
                dataType: domain.DataType,
                region: domain.Region,
                documentType: domain.DocumentType,
                createTimeStamp: domain.CreateTimestamp,
                asOfDate: domain.AsOfDate,
                asOfTime: domain.AsOfTime,
                tags: domain.Tags,
                price: domain.Price,
                side: domain.Side
            );

            return dto;
        }

        public FxSpotPriceData ToDomain(FxSpotPriceDataDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var domain = new FxSpotPriceData
            {
                SchemaVersion = dto.SchemaVersion,
                Version = dto.Version,
                AssetId = dto.AssetId, // This will be auto-normalized to lowercase
                DisplayAssetId = dto.AssetId, // Preserve the original case
                AssetClass = dto.AssetClass,
                DataType = dto.DataType,
                Region = dto.Region,
                DocumentType = dto.DocumentType,
                CreateTimestamp = DateTime.UtcNow,
                AsOfDate = dto.AsOfDate,
                AsOfTime = dto.AsOfTime,
                Price = dto.Price,
                Tags = dto.Tags?.ToList() ?? new List<string>(),
                Side = ConvertToDomainSide(dto.Side)
            };

            return domain;
        }

        private static PriceSide ConvertToDomainSide(PriceSideDto? dtoSide)
        {
            if (dtoSide == null)
                return PriceSide.Mid;

            return dtoSide.Value switch
            {
                PriceSideDto.Mid => PriceSide.Mid,
                PriceSideDto.Bid => PriceSide.Bid,
                PriceSideDto.Ask => PriceSide.Ask,
                _ => PriceSide.Mid
            };
        }

        #region Static Methods
        private static readonly FxSpotPriceDataMapper _instance = new();
        public static FxSpotPriceDataDto MapToDto(FxSpotPriceData domain) => _instance.ToDto(domain);

        public static FxSpotPriceData MapToDomain(FxSpotPriceDataDto dto) => _instance.ToDomain(dto);
        #endregion
    }
}