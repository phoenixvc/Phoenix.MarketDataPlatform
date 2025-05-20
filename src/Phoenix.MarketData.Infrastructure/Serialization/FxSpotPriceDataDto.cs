using Phoenix.MarketData.Domain;
using System.Text.Json.Serialization; // <- for System.Text.Json attributes

namespace Phoenix.MarketData.Infrastructure.Serialization;

public class FxSpotPriceDataDto : BaseMarketDataDto
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("side")]
    public PriceSideDto? Side { get; set; } // nullable, null by default

    public FxSpotPriceDataDto()
    {
    }

    [JsonConstructor] // System.Text.Json supports this since .NET 7
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
        Side = side switch
        {
            PriceSide.Mid => PriceSideDto.Mid,
            PriceSide.Bid => PriceSideDto.Bid,
            PriceSide.Ask => PriceSideDto.Ask,
            _ => PriceSideDto.Mid,
        };
    }
}
