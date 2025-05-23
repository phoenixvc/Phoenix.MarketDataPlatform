using System;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Domain.Domain.Events
{
    /// <summary>
    /// Event raised when market data changes
    /// </summary>
    /// <typeparam name="T">Type of market data entity</typeparam>
    public class MarketDataChangedEvent<T> where T : IMarketDataEntity
    {
        /// <summary>
        /// Gets the updated market data
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Gets the timestamp when this event occurred
        /// </summary>
        public DateTimeOffset OccurredAt { get; }

        /// <summary>
        /// Gets the type of change that occurred
        /// </summary>
        public ChangeType ChangeType { get; }

        /// <summary>
        /// Initializes a new instance of the MarketDataChangedEvent class
        /// </summary>
        /// <param name="data">The updated market data</param>
        /// <param name="changeType">The type of change that occurred</param>
        public MarketDataChangedEvent(T data, ChangeType changeType = ChangeType.Updated)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            OccurredAt = DateTimeOffset.UtcNow;
            ChangeType = changeType;
        }
    }

    /// <summary>
    /// Represents the type of change that occurred to market data
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// Data was created
        /// </summary>
        Created,

        /// <summary>
        /// Data was updated
        /// </summary>
        Updated,

        /// <summary>
        /// Data was deleted
        /// </summary>
        Deleted
    }
}