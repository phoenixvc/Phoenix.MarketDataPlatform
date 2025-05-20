using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Core.Events
{
    public interface IMarketDataEventPublisher
    {
        Task PublishDataChangedEventAsync<T>(T marketData) where T : IMarketDataEntity;
    }
}