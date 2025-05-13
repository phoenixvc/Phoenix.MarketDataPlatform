using Newtonsoft.Json;

namespace Phoenix.MarketData.Infrastructure.Serialization;

public class FxSpotPriceDataDto : BaseMarketDataDto
{
    [JsonProperty("price")]
    public required decimal Price { get; set; }
}