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
        private readonly string _baseSourceUri;

        // Event Grid limits
        private const int MaxEventsPerBatch = 100;
        private const int MaxPayloadSizeBytes = 1024 * 1024; // 1MB

        // Constructor for direct DI usage
        public EventGridPublisher(EventGridPublisherClient client, ILogger<EventGridPublisher> logger, string baseSourceUri = "https://phoenix.marketdata/events/")
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseSourceUri = baseSourceUri;
        }

        // Async factory for secret-based construction
        public static async Task<EventGridPublisher> CreateAsync(
            IMarketDataSecretProvider secretProvider,
            ILogger<EventGridPublisher> logger)
        {
            if (secretProvider == null) throw new ArgumentNullException(nameof(secretProvider));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            try
            {
                var endpoint = await secretProvider.GetEventGridEndpointAsync();
                var key = await secretProvider.GetEventGridKeyAsync();

                // Validate secrets
                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    throw new ArgumentException("Event Grid endpoint cannot be null or empty", nameof(endpoint));
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    throw new ArgumentException("Event Grid key cannot be null or empty", nameof(key));
                }

                // Validate endpoint is a valid URI
                if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
                {
                    throw new ArgumentException($"Invalid Event Grid endpoint URI: {endpoint}", nameof(endpoint));
                }

                var client = new EventGridPublisherClient(uri, new AzureKeyCredential(key));
                return new EventGridPublisher(client, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create EventGridPublisher");
                throw;
            }
        }

        // Generic event publish (CloudEvent, batch-safe)
        public async Task PublishAsync<T>(T eventData, string? topic = null) where T : class
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));

            try
            {
                var eventType = typeof(T).Name;
                var source = BuildSourceUri(topic ?? DeriveTopicFromEventType(eventType));

                var data = BinaryData.FromString(JsonSerializer.Serialize(eventData));
                var cloudEvent = new CloudEvent(
                    source.ToString(),
                    eventType,
                    data,
                    "application/json",
                    CloudEventDataFormat.Json
                )
                {
                    Id = Guid.NewGuid().ToString(),
                    Time = DateTimeOffset.UtcNow
                };

                // Add topic as a custom attribute if provided
                if (!string.IsNullOrEmpty(topic))
                {
                    cloudEvent.ExtensionAttributes.Add("topic", topic);
                }

                await _client.SendEventAsync(cloudEvent);
                _logger.LogInformation("Published event {EventType} with source {Source}", eventType, source);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event {EventType}", typeof(T).Name);
                throw;
            }
        }

        // Batch publish with chunking and size limits
        public async Task PublishManyAsync<T>(IEnumerable<T> events, string? topic = null) where T : class
        {
            var eventsList = events?.ToList() ?? new List<T>();
            if (!eventsList.Any())
                return;

            try
            {
                var eventType = typeof(T).Name;
                var source = BuildSourceUri(topic ?? DeriveTopicFromEventType(eventType));

                // Convert all events to CloudEvents first
                var allCloudEvents = eventsList.Select(e =>
                {
                    var cloudEvent = new CloudEvent(
                        source.ToString(),
                        eventType,
                        BinaryData.FromString(JsonSerializer.Serialize(e)),
                        "application/json",
                        CloudEventDataFormat.Json
                    )
                    {
                        Id = Guid.NewGuid().ToString(),
                        Time = DateTimeOffset.UtcNow
                    };

                    // Add topic as a custom attribute if provided
                    if (!string.IsNullOrEmpty(topic))
                    {
                        cloudEvent.ExtensionAttributes.Add("topic", topic);
                    }

                    return cloudEvent;
                }).ToList();

                // Split into batches respecting Event Grid limits
                var batches = SplitIntoBatches(allCloudEvents);
                int totalSent = 0;
                int batchNumber = 0;

                foreach (var batch in batches)
                {
                    batchNumber++;
                    try
                    {
                        await _client.SendEventsAsync(batch);
                        totalSent += batch.Count;
                        _logger.LogInformation("Published batch {BatchNumber}/{TotalBatches} with {Count} events of type {EventType}",
                            batchNumber, batches.Count, batch.Count, eventType);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish batch {BatchNumber}/{TotalBatches} with {Count} events of type {EventType}",
                            batchNumber, batches.Count, batch.Count, eventType);

                        // Continue with other batches but record the failure
                        // Consider implementing retry logic here or raising a specific exception
                        // that allows callers to handle partial failures
                        // For now, we'll just continue with other batches
                    }
                }

                _logger.LogInformation("Published {TotalSent}/{TotalEvents} events of type {EventType} in {BatchCount} batches",
                    totalSent, eventsList.Count, eventType, batches.Count);

                // If we didn't send all events, throw an exception
                if (totalSent < eventsList.Count)
                {
                    throw new EventPublishException($"Failed to publish all events. Only {totalSent} out of {eventsList.Count} were sent.");
                }
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

            try
            {
                await _client.SendEventAsync(eventGridEvent);
                _logger.LogInformation("Published EventGridEvent {EventType} for {Subject}", eventType, eventGridEvent.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish EventGridEvent {EventType} for {Subject}",
                    eventType, eventGridEvent.Subject);
                throw;
            }
        }

        // Helper: PascalCase to kebab-case for topic derivation
        private static string DeriveTopicFromEventType(string eventType)
        {
            return string.Concat(eventType.Select((x, i) => i > 0 && char.IsUpper(x) ? "-" + x.ToString().ToLower() : x.ToString().ToLower()));
        }

        // Helper: Build a proper URI for the CloudEvent source
        private Uri BuildSourceUri(string topic)
        {
            if (Uri.TryCreate(topic, UriKind.Absolute, out var uri))
            {
                return uri;
            }

            // Normalize the topic name for use in a URI
            var normalizedTopic = topic.Replace(" ", "-").ToLowerInvariant();

            // Ensure no double slashes in the URL
            var baseUri = _baseSourceUri.TrimEnd('/');
            normalizedTopic = normalizedTopic.TrimStart('/');

            return new Uri($"{baseUri}/{normalizedTopic}");
        }

        // Helper: Split CloudEvents into right-sized batches
        private List<List<CloudEvent>> SplitIntoBatches(List<CloudEvent> events)
        {
            var result = new List<List<CloudEvent>>();
            var currentBatch = new List<CloudEvent>();
            long currentBatchSize = 0;

            foreach (var cloudEvent in events)
            {
                // Estimate the size of this event
                // This is an approximation since the actual wire size includes headers, etc.
                long eventSize = EstimateEventSize(cloudEvent);

                // If adding this event would exceed either limit, start a new batch
                if (currentBatch.Count >= MaxEventsPerBatch ||
                    (currentBatch.Count > 0 && currentBatchSize + eventSize > MaxPayloadSizeBytes))
                {
                    result.Add(currentBatch);
                    currentBatch = new List<CloudEvent>();
                    currentBatchSize = 0;
                }

                currentBatch.Add(cloudEvent);
                currentBatchSize += eventSize;
            }

            // Add the last batch if it has any events
            if (currentBatch.Count > 0)
            {
                result.Add(currentBatch);
            }

            return result;
        }

        // Helper: Estimate the size of a CloudEvent (rough approximation)
        private long EstimateEventSize(CloudEvent cloudEvent)
        {
            // Base size for id, source, type, etc.
            long size = 200;

            // Add size of data
            if (cloudEvent.Data != null)
            {
                size += cloudEvent.Data.ToArray().Length;
            }

            // Add size for extension attributes
            foreach (var attr in cloudEvent.ExtensionAttributes)
            {
                size += attr.Key.Length + EstimateExtensionAttributeSize(attr.Value);
            }

            return size;
        }

        // Helper: Estimate size of extension attribute value
        private long EstimateExtensionAttributeSize(object? value)
        {
            if (value == null) return 4; // "null"
            return value.ToString()?.Length ?? 0;
        }
    }

    // Custom exception for partial publishing failures
    public class EventPublishException : Exception
    {
        public EventPublishException(string message) : base(message) { }
        public EventPublishException(string message, Exception inner) : base(message, inner) { }
    }
}