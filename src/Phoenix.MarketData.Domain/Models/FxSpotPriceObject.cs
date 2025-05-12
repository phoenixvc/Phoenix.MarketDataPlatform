using Newtonsoft.Json;
using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Domain.Models
{
    /// <summary>
    /// Represents the spot price object for foreign exchange (FX) data. This class provides
    /// properties that describe the details of a specific FX spot price
    /// </summary>
    public class FxSpotPriceObject : BaseMarketDataObject
    {
        // --- Spot-specific Payload ---
        [JsonProperty("price")]
        public required double Price { get; set; }
    }
}