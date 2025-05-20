using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Domain.Events
{
    public interface IMarketDataEventPublisher
    {
        Task PublishDataChangedEventAsync<T>(T marketData) where T : IMarketData;
    }
}