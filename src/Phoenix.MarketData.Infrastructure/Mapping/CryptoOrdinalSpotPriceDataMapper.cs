using Phoenix.MarketData.Core;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Phoenix.MarketData.Infrastructure.Mapping
{
    public class CryptoOrdinalSpotPriceDataMapper : IMarketDataMapper<CryptoOrdinalSpotPriceData, CryptoOrdinalSpotPriceDataDto>
    {
        public CryptoOrdinalSpotPriceDataDto ToDto(CryptoOrdinalSpotPriceData domain)
        {
            ArgumentNullException.ThrowIfNull(domain);

            var dto = new CryptoOrdinalSpotPriceDataDto(
                id: domain.Id,
                schemaVersion: domain.SchemaVersion,
                version: domain.Version,
                assetId: domain.DisplayAssetId, // Use DisplayAssetId to preserve original case
                assetClass: domain.AssetClass,
                dataType: domain.DataType,
                region: domain.Region,
                documentType: domain.DocumentType,
                createTimestamp: domain.CreateTimestamp,
                asOfDate: domain.AsOfDate,
                asOfTime: domain.AsOfTime,
                tags: domain.Tags,
                price: domain.Price,
                side: domain.Side,
                collectionName: domain.CollectionName ?? string.Empty,
                parentInscriptionId: domain.ParentInscriptionId ?? string.Empty,
                inscriptionId: domain.InscriptionId ?? string.Empty,
                inscriptionNumber: domain.InscriptionNumber,
                currency: domain.Currency ?? string.Empty
            );

            return dto;
        }

        public CryptoOrdinalSpotPriceData ToDomain(CryptoOrdinalSpotPriceDataDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Create domain object with required properties
            var domain = new CryptoOrdinalSpotPriceData
            {
                SchemaVersion = dto.SchemaVersion,
                Version = dto.Version,
                AssetId = dto.AssetId, // This will be auto-normalized to lowercase by the property setter
                AssetClass = dto.AssetClass,
                DataType = dto.DataType,
                Region = dto.Region,
                DocumentType = dto.DocumentType,
                CreateTimestamp = DateTime.UtcNow,
                AsOfDate = dto.AsOfDate,
                AsOfTime = dto.AsOfTime,
                Price = dto.Price,
                Side = ConvertToDomainSide(dto.Side),
                CollectionName = dto.CollectionName ?? string.Empty,
                ParentInscriptionId = dto.ParentInscriptionId ?? string.Empty,
                InscriptionId = dto.InscriptionId ?? string.Empty,
                InscriptionNumber = dto.InscriptionNumber,
                Currency = dto.Currency ?? string.Empty
            };

            // Set tags using the SetTags method from BaseMarketData
            if (dto.Tags != null)
            {
                domain.SetTags(dto.Tags);
            }
            else
            {
                domain.SetTags(new List<string>());
            }

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
        private static readonly CryptoOrdinalSpotPriceDataMapper _instance = new();
        public static CryptoOrdinalSpotPriceDataDto MapToDto(CryptoOrdinalSpotPriceData domain) => _instance.ToDto(domain);

        public static CryptoOrdinalSpotPriceData MapToDomain(CryptoOrdinalSpotPriceDataDto dto) => _instance.ToDomain(dto);
        #endregion
    }
}