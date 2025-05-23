using System;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Domain.Domain.Events;

/// <summary>
/// Event raised when new market data is created
/// </summary>
/// <typeparam name="T">Type of market data entity</typeparam>
public class MarketDataCreatedEvent<T> where T : IMarketDataEntity
{
    /// <summary>
    /// Gets the newly created market data
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// Gets the timestamp when this event occurred
    /// </summary>
    public DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Initializes a new instance of the MarketDataCreatedEvent class
    /// </summary>
    /// <param name="data">The newly created market data</param>
    public MarketDataCreatedEvent(T data)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        OccurredAt = DateTimeOffset.UtcNow;
    }
}