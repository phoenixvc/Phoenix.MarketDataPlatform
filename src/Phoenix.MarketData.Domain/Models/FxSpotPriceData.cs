namespace Phoenix.MarketData.Domain.Models
{
    /// <summary>
    /// Represents the spot price object for foreign exchange (FX) data. This class provides
    /// properties that describe the details of a specific FX spot price
    /// </summary>
    public class FxSpotPriceData : BaseMarketData
    {
        /// <summary>
        /// Gets or sets the price value for the foreign exchange (FX) spot data.
        /// The property represents the numerical value of the FX spot price, which is
        /// crucial for determining trading rates and market conditions.
        /// </summary>
        public required decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the price side for the foreign exchange (FX) spot data.
        /// The property represents whether the price corresponds to the "Mid", "Bid", or "Ask" value,
        /// which defines the specific market context of the FX spot price.
        /// </summary>
        public PriceSide Side { get; set; } = PriceSide.Mid;
    }
}