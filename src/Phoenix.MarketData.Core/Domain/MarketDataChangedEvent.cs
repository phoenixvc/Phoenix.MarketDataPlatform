using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Core.Domain.Events
{
    public class MarketDataChangedEvent<T> where T : IMarketDataEntity
    {
        public T Data { get; }
        public DateTimeOffset OccurredAt { get; }

        public MarketDataChangedEvent(T data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            OccurredAt = DateTimeOffset.UtcNow;
        }
    }
}
