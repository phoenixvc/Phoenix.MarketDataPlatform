using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Domain.Models;

/// <summary>
/// Represents an FX Volatility Surface market data object.
/// </summary>
public class FxVolSurfaceData : BaseMarketData
{
    /// <summary>
    /// The unique identifier for the vol surface (could be currency pair, or custom).
    /// </summary>
    public required string SurfaceName { get; set; }

    /// <summary>
    /// Matrix of volatilities [expiry, tenor, volatility].
    /// Key: expiry (e.g., "1M"), value: dictionary of [tenor, vol].
    /// Example: { "1M": { "ATM": 0.115, "25D_Put": 0.121, ... }, ... }
    /// </summary>
    public required Dictionary<string, Dictionary<string, decimal>> SurfaceMatrix { get; set; }

    /// <summary>
    /// Optional: Metadata or any custom field
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
