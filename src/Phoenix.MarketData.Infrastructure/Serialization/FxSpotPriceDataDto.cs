using Phoenix.MarketData.Core;
using System.Text.Json.Serialization; // <- for System.Text.Json attributes

namespace Phoenix.MarketData.Infrastructure.Serialization;

public class FxSpotPriceDataDto : BaseMarketDataDto
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("side")]
    public PriceSideDto? Side { get; set; } // nullable, defaults to Mid in constructor

    [JsonConstructor]
    public FxSpotPriceDataDto(
        string id,
        string schemaVersion,
        int? version,
        string assetId,
        string assetClass,
        string dataType,
        string region,
        string documentType,
        DateTimeOffset createTimeStamp,
        DateOnly asOfDate,
        TimeOnly? asOfTime,
        List<string> tags,
        decimal price,
        PriceSide side = PriceSide.Mid
    ) : base(id, schemaVersion, version, assetId, assetClass, dataType, region, documentType, createTimeStamp, asOfDate, asOfTime, tags)
    {
        Price = price;
        Side = (PriceSideDto)side;
    }
}
