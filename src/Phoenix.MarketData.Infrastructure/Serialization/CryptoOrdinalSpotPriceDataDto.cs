using Newtonsoft.Json;
using Phoenix.MarketData.Domain;

namespace Phoenix.MarketData.Infrastructure.Serialization;

public class CryptoOrdinalSpotPriceDataDto : BaseMarketDataDto
{
    [JsonProperty("price")]
    public decimal Price { get; set; }
    
    [JsonProperty("side")]
    public PriceSideDto Side { get; set; }

    [JsonProperty("inscriptionNumber")]
    public int InscriptionNumber { get; set; }

    [JsonProperty("inscriptionId")]
    public string InscriptionId { get; set; }
    
    [JsonProperty("parentInscriptionId")]
    public string ParentInscriptionId { get; set; }

    [JsonProperty("collectionName")]
    public string CollectionName { get; set; }

    public CryptoOrdinalSpotPriceDataDto()
    {
    }
    
    public CryptoOrdinalSpotPriceDataDto(string id, string schemaVersion, int? version,
        string assetId, string assetClass, string dataType, string region, string documentType,
        DateTimeOffset createTimestamp, DateOnly asOfDate, TimeOnly? asOfTime, List<string> tags,
        decimal price, PriceSide? side, string collectionName, string parentInscriptionId,
        string inscriptionId, int inscriptionNumber) : base(id, schemaVersion, version, assetId, assetClass, dataType,
        region, documentType, createTimestamp, asOfDate, asOfTime, tags)
    {
        Price = price;
        Side = side switch
        {
            PriceSide.Mid => PriceSideDto.Mid,
            PriceSide.Bid => PriceSideDto.Bid,
            PriceSide.Ask => PriceSideDto.Ask,
            _ => PriceSideDto.Mid,
        };
        CollectionName = collectionName;
        ParentInscriptionId = parentInscriptionId;
        InscriptionId = inscriptionId;
        InscriptionNumber = inscriptionNumber;
    }
}