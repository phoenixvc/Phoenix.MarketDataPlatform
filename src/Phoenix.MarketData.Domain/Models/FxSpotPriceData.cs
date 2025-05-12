namespace Phoenix.MarketData.Domain.Models
{
    /// <summary>
    /// Represents the spot price object for foreign exchange (FX) data. This class provides
    /// properties that describe the details of a specific FX spot price
    /// </summary>
    public class FxSpotPriceData : BaseMarketData
    {
        // --- Spot-specific Payload ---
        public required decimal Price { get; set; }
    }
}