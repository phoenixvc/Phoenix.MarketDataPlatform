using Newtonsoft.Json;

namespace Phoenix.MarketData.Infrastructure.Serialization;

public class FxSpotPriceDataDto : BaseMarketDataDto
{
    [JsonProperty("price")]
    public decimal Price { get; set; }

    public FxSpotPriceDataDto(string id, string schemaVersion, int? version, string assetId, string assetClass, 
        string dataType, string region, string documentType, DateTimeOffset createTimeStamp, DateOnly asOfDate,
        TimeOnly? asOfTime, List<string> tags, decimal price) : base(id, schemaVersion, version, assetId, assetClass, dataType, region, documentType, createTimeStamp, asOfDate, asOfTime, tags)
    {
        Price = price;
    }
}