using Newtonsoft.Json;

namespace Phoenix.MarketData.Infrastructure.Serialization;

public class FxSpotPriceObjectDto : BaseMarketDataDto
{
    [JsonProperty("price")]
    public decimal Price { get; set; }
}