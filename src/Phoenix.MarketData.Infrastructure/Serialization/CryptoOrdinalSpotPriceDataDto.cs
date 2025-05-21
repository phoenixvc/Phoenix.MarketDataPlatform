using Phoenix.MarketData.Core;
using System.Text.Json.Serialization;

namespace Phoenix.MarketData.Infrastructure.Serialization;

public class CryptoOrdinalSpotPriceDataDto : BaseMarketDataDto
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    [JsonPropertyName("side")]
    public PriceSideDto Side { get; set; } = PriceSideDto.Mid;
    [JsonPropertyName("inscriptionNumber")]
    public int InscriptionNumber { get; set; }
    [JsonPropertyName("inscriptionId")]
    public string InscriptionId { get; set; } = string.Empty;
    [JsonPropertyName("parentInscriptionId")]
    public string ParentInscriptionId { get; set; } = string.Empty;
    [JsonPropertyName("collectionName")]
    public string CollectionName { get; set; } = string.Empty;

    [JsonConstructor]
    public CryptoOrdinalSpotPriceDataDto(
        string id,
        string schemaVersion,
        int? version,
        string assetId,
        string assetClass,
        string dataType,
        string region,
        string documentType,
        DateTimeOffset createTimestamp,
        DateOnly asOfDate,
        TimeOnly? asOfTime,
        List<string> tags,
        decimal price,
        PriceSide? side,
        string collectionName,
        string parentInscriptionId,
        string inscriptionId,
        int inscriptionNumber,
        string currency
    ) : base(id, schemaVersion, version, assetId, assetClass, dataType, region, documentType, createTimestamp, asOfDate, asOfTime, tags)
    {
        Price = price;
        Side = side switch
        {
            PriceSide.Mid => PriceSideDto.Mid,
            PriceSide.Bid => PriceSideDto.Bid,
            PriceSide.Ask => PriceSideDto.Ask,
            null => PriceSideDto.Mid,
            _ => throw new ArgumentOutOfRangeException(nameof(side))
        };
        CollectionName = collectionName;
        ParentInscriptionId = parentInscriptionId;
        InscriptionId = inscriptionId;
        InscriptionNumber = inscriptionNumber;
        Currency = currency;
    }
}