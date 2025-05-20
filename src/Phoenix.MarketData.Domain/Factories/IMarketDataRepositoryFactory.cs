using Phoenix.MarketData.Domain.Repositories;

namespace Phoenix.MarketData.Domain.Factories
{
    public interface IMarketDataRepositoryFactory
    {
        IMarketDataRepository CreateRepository();
    }
}