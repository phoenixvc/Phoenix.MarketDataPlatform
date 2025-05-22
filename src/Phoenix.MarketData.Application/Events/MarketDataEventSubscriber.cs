using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Phoenix.MarketData.Core.Events;
using Phoenix.MarketData.Application.Services;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Core.Events.IntegrationEvents;

namespace Phoenix.MarketData.Application.Events
{
    /// <summary>
    /// Subscribes to market data events and processes them with retry capability
    /// </summary>
    public class MarketDataEventSubscriber : IMarketDataEventSubscriber
    {
        private readonly IMarketDataEventProcessor _eventProcessor;
        private readonly ILogger<MarketDataEventSubscriber> _logger;
        private readonly IAsyncPolicy _retryPolicy;

        public MarketDataEventSubscriber(
            IMarketDataEventProcessor eventProcessor,
            ILogger<MarketDataEventSubscriber> logger)
        {
            _eventProcessor = eventProcessor ?? throw new ArgumentNullException(nameof(eventProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Configure retry policy with exponential backoff
            _retryPolicy = Policy
                .Handle<Exception>(ex => !(ex is OperationCanceledException))
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Error processing market data event. Retry attempt {RetryCount} after {RetryTimeSpan}...",
                            retryCount, timeSpan);
                    });
        }

        /// <summary>
        /// Handles market data created events with retry logic
        /// </summary>
        public async Task HandleMarketDataCreatedEventAsync(MarketDataCreatedIntegrationEvent eventData, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Received market data created event for {DataType} {AssetId}",
                eventData.DataType, eventData.AssetId);

            await _retryPolicy.ExecuteAsync(async (ctx, ct) =>
            {
                try
                {
                    // Process event
                    _logger.LogInformation("Processing market data created event: {EventId}", eventData.Id);

                    // Call the event processor to handle the event
                    await _eventProcessor.ProcessCreatedEventAsync(eventData, cancellationToken);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    _logger.LogError(ex, "Error processing market data created event: {EventId}", eventData.Id);
                    throw; // Rethrowing to trigger retry
                }
            });
        }

        /// <summary>
        /// Handles market data changed events with retry logic
        /// </summary>
        public async Task HandleMarketDataChangedEventAsync(MarketDataChangedIntegrationEvent eventData, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Received market data changed event for {DataType} {AssetId}",
                eventData.DataType, eventData.AssetId);

            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    // Process event
                    _logger.LogInformation("Processing market data changed event: {EventId}", eventData.Id);

                    // Call the event processor to handle the event
                    await _eventProcessor.ProcessChangedEventAsync(eventData, cancellationToken);
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    _logger.LogError(ex, "Error processing market data changed event: {EventId}", eventData.Id);
                    throw; // Rethrowing to trigger retry
                }
            });
        }
    }
}