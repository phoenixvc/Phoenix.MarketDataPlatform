using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Core.Events;
using Phoenix.MarketData.Domain.Configuration;
using Phoenix.MarketData.Infrastructure.Configuration;

namespace Phoenix.MarketData.Infrastructure.Events
{
    public class EventGridPublisher : IEventPublisher
    {
        private readonly EventGridPublisherClient _client;
        private readonly ILogger<EventGridPublisher> _logger;

        // Private constructor: forces use of factory method.
        private EventGridPublisher(EventGridPublisherClient client, ILogger<EventGridPublisher> logger)
        {
            _client = client;
            _logger = logger;
        }

        // The proper async factory.
        public static async Task<EventGridPublisher> CreateAsync(
            IMarketDataSecretProvider secretProvider,
            ILogger<EventGridPublisher> logger)
        {
            var endpoint = await secretProvider.GetEventGridEndpointAsync();
            var key = await secretProvider.GetEventGridKeyAsync();
            var client = new EventGridPublisherClient(new Uri(endpoint), new AzureKeyCredential(key));
            return new EventGridPublisher(client, logger);
        }

        public async Task PublishAsync<T>(T eventData, string? topic = null) where T : class
        {
            try
            {
                var eventType = typeof(T).Name;
                topic ??= DeriveTopicFromEventType(eventType);

                var data = BinaryData.FromString(JsonSerializer.Serialize(eventData));
                var cloudEvent = new CloudEvent(
                    topic,
                    eventType,
                    data,
                    "application/json",          // <-- This is correct
                    CloudEventDataFormat.Json    // <-- Optional, makes intent clear
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
                })
                    .ToList();

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


        private static string DeriveTopicFromEventType(string eventType)
        {
            // Simple PascalCase to kebab-case conversion
            return string.Concat(eventType.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString().ToLower() : x.ToString().ToLower()));
        }
    }
}
