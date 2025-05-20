
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Core.Domain.Events;

public class MarketDataCreatedEvent<T> where T : IMarketDataEntity
{
    public T Data { get; }
    public DateTimeOffset OccurredAt { get; }

    public MarketDataCreatedEvent(T data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        OccurredAt = DateTimeOffset.UtcNow;
    }
}