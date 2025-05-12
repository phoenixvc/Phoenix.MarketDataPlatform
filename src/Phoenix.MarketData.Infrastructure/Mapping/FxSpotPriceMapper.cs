using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Mapping;

public static class FxSpotPriceMapper
{
    public static FxSpotPriceDto ToDto(FxSpotPriceData domain)
    {
        ArgumentNullException.ThrowIfNull(domain);
        
        var dto = new FxSpotPriceDto
        {
            Price = domain.Price,
        };

        // Apply base properties
        var baseDto = BaseMarketDataMapper.ToDto(domain);
        dto.CopyBasePropertiesFrom(baseDto);

        return dto;
    }
    
    public static void ApplyToDomain(FxSpotPriceDto dto, FxSpotPriceData domain) 
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(domain);

        domain.Price = dto.Price;

        BaseMarketDataMapper.ApplyToDomain(dto, domain);
    }
}