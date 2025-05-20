using System;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Models.Interfaces;
using Azure.Messaging.EventGrid;
using System.Text.Json;

namespace Phoenix.MarketData.Infrastructure.Events
{
    public class MarketDataEventPublisher : IMarketDataEventPublisher
    {
        private readonly EventGridPublisherClient _eventGridClient;
        private const string DataChangedEventType = "Phoenix.MarketData.DataChanged";
        private const string DataCreatedEventType = "Phoenix.MarketData.DataCreated";

        public MarketDataEventPublisher(EventGridPublisherClient eventGridClient)
        {
            _eventGridClient = eventGridClient ?? throw new ArgumentNullException(nameof(eventGridClient));
        }

        public async Task PublishDataChangedEventAsync<T>(T marketData) where T : IMarketData
        {
            var eventType = marketData.Version.Value > 1 ? DataChangedEventType : DataCreatedEventType;
            
            var eventData = new BinaryData(JsonSerializer.Serialize(marketData));
            var eventGridEvent = new EventGridEvent(
                subject: $"{marketData.DataType}.{marketData.AssetClass}/{marketData.AssetId}",
                eventType: eventType,
                dataVersion: marketData.SchemaVersion,
                data: eventData);

            await _eventGridClient.SendEventAsync(eventGridEvent);
        }
    }
}