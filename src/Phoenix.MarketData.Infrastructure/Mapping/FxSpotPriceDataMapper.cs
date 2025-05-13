using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Serialization;

namespace Phoenix.MarketData.Infrastructure.Mapping;

public static class FxSpotPriceDataMapper
{
    public static FxSpotPriceDataDto ToDto(FxSpotPriceData domain)
    {
        ArgumentNullException.ThrowIfNull(domain);
        
        var dto = new FxSpotPriceDataDto
        {
            Price = domain.Price,
        };

        // Apply base properties
        var baseDto = BaseMarketDataMapper.ToDto(domain);
        dto.CopyBasePropertiesFrom(baseDto);

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