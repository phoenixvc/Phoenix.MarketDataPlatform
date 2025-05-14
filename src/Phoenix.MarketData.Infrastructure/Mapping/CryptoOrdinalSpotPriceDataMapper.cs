using Phoenix.MarketData.Domain;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Serialization;

namespace Phoenix.MarketData.Infrastructure.Mapping;

public static class CryptoOrdinalSpotPriceDataMapper
{
    public static CryptoOrdinalSpotPriceDataDto ToDto(CryptoOrdinalSpotPriceData domain)
    {
        ArgumentNullException.ThrowIfNull(domain);
        
        var dto = new CryptoOrdinalSpotPriceDataDto(domain.Id, domain.SchemaVersion, domain.Version,
            domain.AssetId, domain.AssetClass, domain.DataType, domain.Region, domain.DocumentType,
            domain.CreateTimestamp, domain.AsOfDate, domain.AsOfTime, domain.Tags, domain.Price, domain.Side,
            domain.CollectionName, domain.ParentInscriptionId, domain.InscriptionId, domain.InscriptionNumber);

        return dto;
    }
    
    public static CryptoOrdinalSpotPriceData ToDomain(CryptoOrdinalSpotPriceDataDto dto) 
    {
        ArgumentNullException.ThrowIfNull(dto);

        var domain = new CryptoOrdinalSpotPriceData
        {
            SchemaVersion = dto.SchemaVersion,
            Version = dto.Version,
            AssetId = dto.AssetId,
            AssetClass = dto.AssetClass,
            DataType = dto.DataType,
            Region = dto.Region,
            DocumentType = dto.DocumentType,
            CreateTimestamp = dto.CreateTimestamp ?? DateTime.UtcNow,
            AsOfDate = dto.AsOfDate,
            AsOfTime = dto.AsOfTime,
            Price = dto.Price,
            Tags = dto.Tags?.ToList() ?? new List<string>(),
            Side = dto.Side switch
            {
                PriceSideDto.Mid => PriceSide.Mid,
                PriceSideDto.Bid => PriceSide.Bid,
                PriceSideDto.Ask => PriceSide.Ask,
                _ => PriceSide.Mid,
            },
            CollectionName = dto.CollectionName,
            ParentInscriptionId = dto.ParentInscriptionId,
            InscriptionId = dto.InscriptionId,
            InscriptionNumber = dto.InscriptionNumber,
        };

        return domain;
    }
}