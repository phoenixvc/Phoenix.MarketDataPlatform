using Phoenix.MarketData.Domain.Constants;

namespace Phoenix.MarketData.Domain.Models;

public class FxSpotPriceData : BaseMarketData
{
    public decimal Price { get; set; }
    public PriceSide Side { get; set; } = PriceSide.Mid;

    public string Currency { get; set; } = CurrencyCodes.USD;

}