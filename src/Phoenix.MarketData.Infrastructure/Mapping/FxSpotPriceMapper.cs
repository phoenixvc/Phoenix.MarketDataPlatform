using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Mapping;

public static class FxSpotPriceMapper
{
    public static FxSpotPriceDto ToDto(FxSpotPriceData domain)
    {
        var dto = new FxSpotPriceDto
        {
            Price = domain.Price,
        };

        // Apply base properties
        var baseDto = BaseMarketDataMapper.ToDto(domain);
        dto.CopyBasePropertiesFrom(baseDto);

        return dto;
    }
}