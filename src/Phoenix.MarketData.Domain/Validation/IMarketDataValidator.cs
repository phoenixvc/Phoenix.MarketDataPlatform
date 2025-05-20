
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Core.Validation
{
    public interface IMarketDataValidator
    {
        void ValidateMarketData<T>(T marketData) where T : IMarketDataEntity;
    }
}