using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Phoenix.MarketData.Application.Services.Decorators;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
using Xunit;

namespace Phoenix.MarketData.Application.Tests.Services.Decorators
{
    public class ValidationMarketDataServiceDecoratorTests
    {
        private readonly Mock<IMarketDataService<TestMarketData>> _mockService;
        private readonly Mock<IValidator<TestMarketData>> _mockValidator;
        private readonly ValidationMarketDataServiceDecorator<TestMarketData> _decorator;
        private readonly TestMarketData _testData;

        public ValidationMarketDataServiceDecoratorTests()
        {
            _mockService = new Mock<IMarketDataService<TestMarketData>>();
            _mockValidator = new Mock<IValidator<TestMarketData>>();
            _decorator = new ValidationMarketDataServiceDecorator<TestMarketData>(
                _mockService.Object,
                _mockValidator.Object);

            _testData = new TestMarketData
            {
                Id = "test-1",
                AssetId = "AAPL",
                AssetClass = "Equity",
                DataType = "Price",
                Region = "US",
                AsOfDate = DateOnly.FromDateTime(DateTime.Today),
                DocumentType = "Daily"
            };
        }

        [Fact]
        public void Constructor_NullValidator_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ValidationMarketDataServiceDecorator<TestMarketData>(
                    _mockService.Object,
                    null!));
        }

        [Fact]
        public async Task PublishMarketDataAsync_ValidationSuccess_CallsDecoratedService()
        {
            // Arrange
            _mockValidator
                .Setup(v => v.ValidateAsync(_testData))
                .ReturnsAsync(ValidationResult.Success());

            _mockService
                .Setup(s => s.PublishMarketDataAsync(_testData))
                .ReturnsAsync("success-id");

            // Act
            var result = await _decorator.PublishMarketDataAsync(_testData);

            // Assert
            Assert.Equal("success-id", result);
            _mockValidator.Verify(v => v.ValidateAsync(_testData), Times.Once);
            _mockService.Verify(s => s.PublishMarketDataAsync(_testData), Times.Once);
        }

        [Fact]
        public async Task PublishMarketDataAsync_ValidationFailure_ThrowsValidationException()
        {
            // Arrange
            var errors = new List<ValidationError>
            {
                new ValidationError { ErrorMessage = "Test error" }
            };

            _mockValidator
                .Setup(v => v.ValidateAsync(_testData))
                .ReturnsAsync(ValidationResult.Failure(errors));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _decorator.PublishMarketDataAsync(_testData));

            Assert.Equal(errors, exception.Errors);
            _mockValidator.Verify(v => v.ValidateAsync(_testData), Times.Once);
            _mockService.Verify(s => s.PublishMarketDataAsync(It.IsAny<TestMarketData>()), Times.Never);
        }

        [Fact]
        public async Task GetLatestMarketDataAsync_BypassesValidation_CallsDecoratedService()
        {
            // Arrange
            var expected = new TestMarketData();
            _mockService
                .Setup(s => s.GetLatestMarketDataAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<string>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _decorator.GetLatestMarketDataAsync(
                "Price", "Equity", "AAPL", "US", DateOnly.FromDateTime(DateTime.Today), "Daily");

            // Assert
            Assert.Same(expected, result);
            _mockValidator.Verify(v => v.ValidateAsync(It.IsAny<TestMarketData>()), Times.Never);
            _mockService.Verify(s => s.GetLatestMarketDataAsync(
                "Price", "Equity", "AAPL", "US", DateOnly.FromDateTime(DateTime.Today), "Daily"),
                Times.Once);
        }

        [Fact]
        public async Task QueryMarketDataAsync_BypassesValidation_CallsDecoratedService()
        {
            // Arrange
            var expected = new List<TestMarketData> { new TestMarketData() };
            _mockService
                .Setup(s => s.QueryMarketDataAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var fromDate = DateTime.UtcNow.AddDays(-7);
            var toDate = DateTime.UtcNow;
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _decorator.QueryMarketDataAsync(
                "Price", "Equity", "AAPL", fromDate, toDate, cancellationToken);

            // Assert
            Assert.Same(expected, result);
            _mockValidator.Verify(v => v.ValidateAsync(It.IsAny<TestMarketData>()), Times.Never);
            _mockService.Verify(s => s.QueryMarketDataAsync(
                "Price", "Equity", "AAPL", fromDate, toDate, cancellationToken),
                Times.Once);
        }

        // Test entity class
        private class TestMarketData : IMarketDataEntity
        {
            public string Id { get; set; } = string.Empty;
            public string DataType { get; set; } = string.Empty;
            public string AssetClass { get; set; } = string.Empty;
            public string AssetId { get; set; } = string.Empty;
            public string Region { get; set; } = string.Empty;
            public DateOnly AsOfDate { get; set; }
            public TimeOnly? AsOfTime { get; set; }
            public string DocumentType { get; set; } = string.Empty;
        }
    }
}