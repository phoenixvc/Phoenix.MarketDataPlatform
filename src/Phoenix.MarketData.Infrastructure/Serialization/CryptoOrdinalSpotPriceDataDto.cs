using Phoenix.MarketData.Domain;
using System.Text.Json.Serialization; // <- ADD THIS

namespace Phoenix.MarketData.Infrastructure.Serialization;

public class CryptoOrdinalSpotPriceDataDto : BaseMarketDataDto
{
    [JsonPropertyName("price")]
    public required decimal Price { get; set; }

    [JsonPropertyName("currency")]
    public required string Currency { get; set; }

    [JsonPropertyName("side")]
    public required PriceSideDto Side { get; set; }

    [JsonPropertyName("inscriptionNumber")]
    public required int InscriptionNumber { get; set; }

    [JsonPropertyName("inscriptionId")]
    public required string InscriptionId { get; set; }

    [JsonPropertyName("parentInscriptionId")]
    public required string ParentInscriptionId { get; set; }

    [JsonPropertyName("collectionName")]
    public required string CollectionName { get; set; }

    public CryptoOrdinalSpotPriceDataDto()
    {
    }

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
        string currency)
        : base(id, schemaVersion, version, assetId, assetClass, dataType, region, documentType, createTimestamp, asOfDate, asOfTime, tags)
    {
        Price = price;
        Side = side switch
        {
            PriceSide.Mid => PriceSideDto.Mid,
            PriceSide.Bid => PriceSideDto.Bid,
            PriceSide.Ask => PriceSideDto.Ask,
            null => PriceSideDto.Mid,
            _ => throw new ArgumentOutOfRangeException(nameof(side), $"Unexpected side value: {side}")
        };
        CollectionName = collectionName;
        ParentInscriptionId = parentInscriptionId;
        InscriptionId = inscriptionId;
        InscriptionNumber = inscriptionNumber;
        Currency = currency;
    }
}
