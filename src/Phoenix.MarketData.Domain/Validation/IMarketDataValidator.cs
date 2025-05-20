using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Domain.Validation
{
    public interface IMarketDataValidator
    {
        void ValidateMarketData<T>(T marketData) where T : IMarketData;
    }
}