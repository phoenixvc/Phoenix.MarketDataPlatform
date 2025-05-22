using System;

namespace Phoenix.MarketData.Core.Models;

/// <summary>
/// Represents spot price data for cryptocurrency ordinals/inscriptions.
/// </summary>
/// <remarks>
/// Ordinals are digital artifacts inscribed on a satoshi (the smallest unit of Bitcoin).
/// This class stores pricing information for these digital assets along with their inscription metadata.
/// </remarks>
public class CryptoOrdinalSpotPriceData : BaseMarketData
{
    /// <summary>
    /// Gets or sets the spot price of the ordinal.
    /// </summary>
    public required decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the currency in which the price is denominated (e.g., "USD", "BTC").
    /// </summary>
    public required string Currency { get; set; }

    /// <summary>
    /// Gets or sets the side of the price (bid, ask, or mid).
    /// </summary>
    public PriceSide? Side { get; set; }

    /// <summary>
    /// Gets or sets the name of the collection this ordinal belongs to.
    /// </summary>
    /// <remarks>
    /// Collections group related ordinals together, similar to NFT collections.
    /// </remarks>
    public required string CollectionName { get; set; }

    /// <summary>
    /// Gets or sets the parent inscription ID, which is typically the collection's
    /// inscription ID or a reference to a parent in hierarchical relationships.
    /// </summary>
    public required string ParentInscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the unique inscription ID of this ordinal.
    /// </summary>
    /// <remarks>
    /// This is typically a transaction hash + output index that identifies where the inscription was created.
    /// </remarks>
    public required string InscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the sequential number of this inscription in the blockchain.
    /// </summary>
    /// <remarks>
    /// Inscription numbers are assigned sequentially as inscriptions are created on the blockchain,
    /// and are often used as a human-friendly reference (e.g., "Inscription #123").
    /// </remarks>
    public int InscriptionNumber { get; set; }
}