using System.Runtime.Serialization;

namespace Phoenix.MarketData.Infrastructure.Serialization;

/// <summary>
/// Represents the side of a price in market data.
/// </summary>
public enum PriceSideDto
{
    /// <summary>
    /// Represents the midpoint pricing side in market data.
    /// Typically used to denote the average or middle value
    /// between the bid and ask prices.
    /// </summary>
    [EnumMember(Value = "Mid")] Mid = 0,

    /// <summary>
    /// Represents the bid pricing side in market data.
    /// Commonly used to indicate the highest price a buyer is willing to pay for an asset.
    /// </summary>
    [EnumMember(Value = "Bid")] Bid = 1,

    /// <summary>
    /// Represents the ask pricing side in market data,
    /// typically referring to the price at which a seller is willing to sell an asset.
    /// </summary>
    [EnumMember(Value = "Ask")] Ask = 2,
}