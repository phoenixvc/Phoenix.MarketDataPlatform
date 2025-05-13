using Newtonsoft.Json;
using Phoenix.MarketData.Domain;

namespace Phoenix.MarketData.Infrastructure.Serialization;

public class FxSpotPriceDataDto : BaseMarketDataDto
{
    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("side")]
    public PriceSideDto Side { get; set; }
    
    public FxSpotPriceDataDto(string id, string schemaVersion, int? version, string assetId, string assetClass, 
        string dataType, string region, string documentType, DateTimeOffset createTimeStamp, DateOnly asOfDate,
        TimeOnly? asOfTime, List<string> tags, decimal price, PriceSide side = PriceSide.Mid) : 
            base(id, schemaVersion, version, assetId, assetClass, dataType, region, documentType, createTimeStamp, asOfDate, asOfTime, tags)
    {
        Price = price;
        Side = side switch
        {
            PriceSide.Mid => PriceSideDto.Mid,
            PriceSide.Bid => PriceSideDto.Bid,
            PriceSide.Ask => PriceSideDto.Ask,
            _ => PriceSideDto.Mid,
        };
    }
}