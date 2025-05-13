namespace Phoenix.MarketData.Domain.Models;

public class CryptoOrdinalSpotPriceData : BaseMarketData
{
    /// <summary>
    /// Represents the monetary value associated with the crypto ordinal spot price data.
    /// </summary>
    public required decimal Price { get; set; }
    
    /// <summary>
    /// The side of the price quote, e.g., mid, bid, or ask.
    /// </summary>
    public PriceSide? Side { get; set; }

    /// <summary>
    /// The name of the collection, used for internal identification.
    /// </summary>
    public required string CollectionName { get; set; }

    /// <summary>
    /// The inscription id of the parent (collection).
    /// </summary>
    public required string ParentInscriptionId { get; set; }

    /// <summary>
    /// The inscription id of the asset.
    /// </summary>
    public required string InscriptionId { get; set; }

    /// <summary>
    /// The inscription number of the asset.
    /// </summary>
    public int InscriptionNumber { get; set; }
}