using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Core.Events.IntegrationEvents;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Application.Services
{
    public interface IMarketDataEventProcessor
    {
        Task ProcessCreatedEventAsync(IMarketDataIntegrationEvent createdEvent, CancellationToken cancellationToken = default);
        Task ProcessChangedEventAsync(IMarketDataIntegrationEvent changedEvent, CancellationToken cancellationToken = default);
    }

    public class MarketDataEventProcessor : IMarketDataEventProcessor
    {
        private readonly IMarketDataService<IMarketDataEntity> _marketDataService;
        private readonly ILogger<MarketDataEventProcessor> _logger;

        public MarketDataEventProcessor(
            IMarketDataService<IMarketDataEntity> marketDataService,
            ILogger<MarketDataEventProcessor> logger)
        {
            _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ProcessCreatedEventAsync(IMarketDataIntegrationEvent createdEvent, CancellationToken cancellationToken = default)
        {
            // Implementation to process created events
            _logger.LogInformation("Processing created event: {EventId}", createdEvent.Id);

            try
            {
                // Example implementation - actual logic will depend on your requirements
                // This might involve fetching data, transforming it, and updating repositories

                // For example, you could query existing data:
                var existingData = await _marketDataService.QueryMarketDataAsync(
                    createdEvent.DataType,
                    createdEvent.AssetClass,
                    createdEvent.AssetId,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Found {Count} existing records for {AssetId}",
                    existingData?.Count() ?? 0, createdEvent.AssetId);

                // Additional processing logic as needed
                // This could include:
                // - Data validation or enrichment
                // - Notifications to other systems
                // - Cache updates
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing created event {EventId} for {AssetId}",
                    createdEvent.Id, createdEvent.AssetId);
                throw; // Re-throw for error handling at higher levels
            }
        }

        public async Task ProcessChangedEventAsync(IMarketDataIntegrationEvent changedEvent, CancellationToken cancellationToken = default)
        {
            // Implementation to process changed events
            _logger.LogInformation("Processing changed event: {EventId}", changedEvent.Id);

            try
            {
                // Example implementation - actual logic will depend on your requirements
                // This might involve fetching data, comparing versions, and updating repositories

                // For example, you might need to get the latest data to compare with the event:
                if (DateTime.TryParse(changedEvent.Timestamp.ToString(), out var eventDate))
                {
                    var dateOnly = new DateOnly(eventDate.Year, eventDate.Month, eventDate.Day);

                    var latestData = await _marketDataService.GetLatestMarketDataAsync(
                        changedEvent.DataType,
                        changedEvent.AssetClass,
                        changedEvent.AssetId,
                        "global", // Default region
                        dateOnly,
                        changedEvent.DocumentType);

                    if (latestData != null)
                    {
                        _logger.LogInformation("Found latest data for {AssetId} with version {Version}",
                            changedEvent.AssetId, latestData.Version);

                        // Additional comparison and processing logic
                        // This could include:
                        // - Version comparison to avoid processing outdated events
                        // - Merging or updating data
                        // - Triggering downstream systems
                    }
                    else
                    {
                        _logger.LogWarning("No existing data found for changed event {EventId} for {AssetId}",
                            changedEvent.Id, changedEvent.AssetId);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not parse timestamp {Timestamp} for event {EventId}",
                        changedEvent.Timestamp, changedEvent.Id);
                }

                // Additional processing logic as needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing changed event {EventId} for {AssetId}",
                    changedEvent.Id, changedEvent.AssetId);
                throw; // Re-throw for error handling at higher levels
            }
        }
    }
}