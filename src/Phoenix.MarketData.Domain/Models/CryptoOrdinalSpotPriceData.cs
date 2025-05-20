namespace Phoenix.MarketData.Core.Models;

public class CryptoOrdinalSpotPriceData : BaseMarketData
{
    public required decimal Price { get; set; }
    public required string Currency { get; set; }
    public PriceSide? Side { get; set; }
    public required string CollectionName { get; set; }
    public required string ParentInscriptionId { get; set; }
    public required string InscriptionId { get; set; }
    public int InscriptionNumber { get; set; }
}
