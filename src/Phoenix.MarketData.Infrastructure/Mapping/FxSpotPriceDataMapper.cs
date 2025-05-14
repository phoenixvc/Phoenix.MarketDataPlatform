using Phoenix.MarketData.Domain;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Serialization;

namespace Phoenix.MarketData.Infrastructure.Mapping;

public static class FxSpotPriceDataMapper
{
    public static FxSpotPriceDataDto ToDto(FxSpotPriceData domain)
    {
        ArgumentNullException.ThrowIfNull(domain);
        
        var dto = new FxSpotPriceDataDto(domain.Id, domain.SchemaVersion, domain.Version,
            domain.AssetId, domain.AssetClass, domain.DataType, domain.Region, domain.DocumentType,
            domain.CreateTimestamp, domain.AsOfDate, domain.AsOfTime, domain.Tags, domain.Price, domain.Side);

        return dto;
    }
    
    public static FxSpotPriceData ToDomain(FxSpotPriceDataDto dto) 
    {
        ArgumentNullException.ThrowIfNull(dto);

        var domain = new FxSpotPriceData
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
            }
        };

        return domain;
    }
}