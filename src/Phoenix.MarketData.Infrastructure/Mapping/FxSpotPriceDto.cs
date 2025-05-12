using Newtonsoft.Json;
using Phoenix.MarketData.Infrastructure.Serialization;

namespace Phoenix.MarketData.Infrastructure.Mapping;

public class FxSpotPriceDto : BaseMarketDataDto
{
    [JsonProperty("price")]
    public required decimal Price { get; set; }
}