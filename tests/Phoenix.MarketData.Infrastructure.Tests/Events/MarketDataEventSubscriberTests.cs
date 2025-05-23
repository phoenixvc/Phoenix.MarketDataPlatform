using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Application.Events;
using Phoenix.MarketData.Application.Services;
using Phoenix.MarketData.Domain.Events;
using Phoenix.MarketData.Domain.Events.IntegrationEvents;
using Phoenix.MarketData.Domain.Models;
using Moq;
using Xunit;

namespace Phoenix.MarketData.Infrastructure.Tests.Events
{
    public class MarketDataEventSubscriberTests
    {
        private Mock<IMarketDataEventProcessor> _mockEventProcessor;
        private Mock<ILogger<MarketDataEventSubscriber>> _mockLogger;
        private MarketDataEventSubscriber _subscriber;

        public MarketDataEventSubscriberTests()
        {
            _mockEventProcessor = new Mock<IMarketDataEventProcessor>();
            _mockLogger = new Mock<ILogger<MarketDataEventSubscriber>>();

            _subscriber = new MarketDataEventSubscriber(
                _mockEventProcessor.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task HandleMarketDataCreatedEvent_ShouldProcessSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();

            // For tests, use object initializer syntax to satisfy required properties
            var createdEvent = new MarketDataCreatedIntegrationEvent
            {
                Id = id,
                AssetId = "EURUSD",
                AssetClass = "fx",
                DataType = "price.spot",
                DocumentType = "official",
                Version = 1,
                Timestamp = DateTimeOffset.UtcNow,
                Region = "GLOBAL" // Added missing required property
            };

            // Set up expectations
            _mockEventProcessor
                .Setup(p => p.ProcessCreatedEventAsync(
                    It.Is<IMarketDataIntegrationEvent>(e =>
                        e.Id == id &&
                        e.AssetId == "EURUSD" &&
                        e.AssetClass == "fx"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _subscriber.HandleMarketDataCreatedEventAsync(createdEvent);

            // Assert
            _mockEventProcessor.Verify(
                p => p.ProcessCreatedEventAsync(
                    It.Is<IMarketDataIntegrationEvent>(e =>
                        e.Id == id &&
                        e.AssetId == "EURUSD" &&
                        e.EventType == "Created"),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Simplify logger verification to avoid nullability issues
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task HandleMarketDataChangedEvent_ShouldProcessSuccessfully()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();

            // For tests, use object initializer syntax to satisfy required properties
            var changedEvent = new MarketDataChangedIntegrationEvent
            {
                Id = id,
                AssetId = "EURUSD",
                AssetClass = "fx",
                DataType = "price.spot",
                DocumentType = "official",
                Version = 2,
                Timestamp = DateTimeOffset.UtcNow,
                Region = "GLOBAL" // Added missing required property
            };

            // Set up expectations
            _mockEventProcessor
                .Setup(p => p.ProcessChangedEventAsync(
                    It.Is<IMarketDataIntegrationEvent>(e =>
                        e.Id == id &&
                        e.AssetId == "EURUSD" &&
                        e.Version == 2),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _subscriber.HandleMarketDataChangedEventAsync(changedEvent);

            // Assert
            _mockEventProcessor.Verify(
                p => p.ProcessChangedEventAsync(
                    It.Is<IMarketDataIntegrationEvent>(e =>
                        e.Id == id &&
                        e.AssetId == "EURUSD" &&
                        e.EventType == "Changed"),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Simplify logger verification to avoid nullability issues
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}