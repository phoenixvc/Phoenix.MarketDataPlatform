using System.Threading;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Events.IntegrationEvents;

namespace Phoenix.MarketData.Domain.Events
{
    /// <summary>
    /// Interface for a service that subscribes to market data events and processes them
    /// </summary>
    public interface IMarketDataEventSubscriber
    {
        /// <summary>
        /// Handles market data created events
        /// </summary>
        /// <param name="eventData">The event data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task HandleMarketDataCreatedEventAsync(MarketDataCreatedIntegrationEvent eventData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Handles market data changed events
        /// </summary>
        /// <param name="eventData">The event data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task HandleMarketDataChangedEventAsync(MarketDataChangedIntegrationEvent eventData, CancellationToken cancellationToken = default);
    }
}