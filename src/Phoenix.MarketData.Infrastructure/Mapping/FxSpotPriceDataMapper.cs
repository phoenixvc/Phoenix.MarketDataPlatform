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
            domain.CreateTimestamp, domain.AsOfDate, domain.AsOfTime, domain.Tags, domain.Price);

        return dto;
    }
    
    public static void ApplyToDomain(FxSpotPriceDataDto dataDto, FxSpotPriceData domain) 
    {
        ArgumentNullException.ThrowIfNull(dataDto);
        ArgumentNullException.ThrowIfNull(domain);

        domain.Price = dataDto.Price;

        BaseMarketDataMapper.ApplyToDomain(dataDto, domain);
    }
}