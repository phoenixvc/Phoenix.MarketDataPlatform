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
        private readonly IMarketDataService<IMarketDataEntity> _marketDataService;
        private readonly ILogger<MarketDataEventSubscriber> _logger;
        private readonly IAsyncPolicy _retryPolicy;

        public MarketDataEventSubscriber(
            IMarketDataService<IMarketDataEntity> marketDataService,
            ILogger<MarketDataEventSubscriber> logger)
        {
            _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
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

            await _retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    // Process event
                    // Note: Actual implementation would depend on your specific business requirements
                    _logger.LogInformation("Processing market data created event: {EventId}", eventData.Id);

                    // Example: You could store in a cache or notify other systems
                    // await _marketDataService.ProcessCreatedEventAsync(eventData);

                    // Prevent warning about not awaiting Task
                    await Task.CompletedTask;
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

                    // Example: You could update caches or notify downstream systems
                    // await _marketDataService.ProcessChangedEventAsync(eventData);

                    // Prevent warning about not awaiting Task
                    await Task.CompletedTask;
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