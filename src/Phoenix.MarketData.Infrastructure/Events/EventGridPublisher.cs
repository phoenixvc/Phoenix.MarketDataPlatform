using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Core.Configuration;
using Phoenix.MarketData.Core.Events;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Events
{
    public class EventGridPublisher : IEventPublisher
    {
        private readonly EventGridPublisherClient _client;
        private readonly ILogger<EventGridPublisher> _logger;

        // Constructor for direct DI usage
        public EventGridPublisher(EventGridPublisherClient client, ILogger<EventGridPublisher> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Async factory for secret-based construction
        public static async Task<EventGridPublisher> CreateAsync(
            IMarketDataSecretProvider secretProvider,
            ILogger<EventGridPublisher> logger)
        {
            var endpoint = await secretProvider.GetEventGridEndpointAsync();
            var key = await secretProvider.GetEventGridKeyAsync();
            var client = new EventGridPublisherClient(new Uri(endpoint), new AzureKeyCredential(key));
            return new EventGridPublisher(client, logger);
        }

        // Generic event publish (CloudEvent, batch-safe)
        public async Task PublishAsync<T>(T eventData, string? topic = null) where T : class
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            try
            {
                var eventType = typeof(T).Name;
                topic ??= DeriveTopicFromEventType(eventType);

                var data = BinaryData.FromString(JsonSerializer.Serialize(eventData));
                var cloudEvent = new CloudEvent(
                    topic,
                    eventType,
                    data,
                    "application/json",
                    CloudEventDataFormat.Json
                )
                {
                    Id = Guid.NewGuid().ToString(),
                    Time = DateTimeOffset.UtcNow
                };

                await _client.SendEventAsync(cloudEvent);
                _logger.LogInformation("Published event {EventType} to {Topic}", eventType, topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event {EventType}", typeof(T).Name);
                throw;
            }
        }

        // Batch publish
        public async Task PublishManyAsync<T>(IEnumerable<T> events, string? topic = null) where T : class
        {
            var eventsList = events.ToList();
            if (!eventsList.Any())
                return;

            try
            {
                var eventType = typeof(T).Name;
                topic ??= DeriveTopicFromEventType(eventType);

                var cloudEvents = eventsList.Select(e => new CloudEvent(
                        topic,
                        eventType,
                        BinaryData.FromString(JsonSerializer.Serialize(e)),
                        "application/json",
                        CloudEventDataFormat.Json
                    )
                {
                    Id = Guid.NewGuid().ToString(),
                    Time = DateTimeOffset.UtcNow
                }).ToList();

                await _client.SendEventsAsync(cloudEvents);
                _logger.LogInformation("Published {Count} events of type {EventType} to {Topic}",
                    eventsList.Count, eventType, topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish events of type {EventType}", typeof(T).Name);
                throw;
            }
        }

        // Market data changed/created "legacy" style event for IMarketData, with subject & event type logic
        public async Task PublishMarketDataEventAsync<T>(T marketData) where T : IMarketDataEntity
        {
            if (marketData == null)
                throw new ArgumentNullException(nameof(marketData));

            var eventType = (marketData.Version.GetValueOrDefault() > 1)
                ? "Phoenix.MarketData.DataChanged"
                : "Phoenix.MarketData.DataCreated";

            var eventData = new BinaryData(JsonSerializer.Serialize(marketData));
            var eventGridEvent = new EventGridEvent(
                subject: $"{marketData.DataType}.{marketData.AssetClass}/{marketData.AssetId}",
                eventType: eventType,
                dataVersion: marketData.SchemaVersion,
                data: eventData
            );

            await _client.SendEventAsync(eventGridEvent);
            _logger.LogInformation("Published EventGridEvent {EventType} for {Subject}", eventType, eventGridEvent.Subject);
        }

        // Helper: PascalCase to kebab-case for topic derivation
        private static string DeriveTopicFromEventType(string eventType)
        {
            return string.Concat(eventType.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString().ToLower() : x.ToString().ToLower()));
        }
    }
}
