using System.Threading.Tasks;

namespace Phoenix.MarketData.Domain.Configuration
{
    public interface IMarketDataSecretProvider
    {
        Task<string> GetCosmosConnectionStringAsync();
        Task<string> GetEventGridKeyAsync();
    }
}