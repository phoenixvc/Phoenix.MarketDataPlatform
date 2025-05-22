using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Phoenix.MarketData.Core.Events
{
    /// <summary>
    /// Defines the contract for publishing domain events
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes an event to the configured event bus
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="eventData">The event data to publish</param>
        /// <param name="topic">Optional topic override</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        Task PublishAsync<T>(T eventData, string? topic = null, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Publishes multiple events to the configured event bus
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="events">Collection of events to publish</param>
        /// <param name="topic">Optional topic override</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        Task PublishManyAsync<T>(IEnumerable<T> events, string? topic = null, CancellationToken cancellationToken = default) where T : class;
    }
}