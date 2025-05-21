using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Application.Services;
using Phoenix.MarketData.Application.Services.Decorators;
using Xunit;

namespace Phoenix.MarketData.Infrastructure.Tests.Validation
{
    public class ValidationDecoratorTests
    {
        private readonly Mock<IMarketDataService<IMarketDataEntity>> _mockMarketDataService;
        private readonly Mock<IValidator<IMarketDataEntity>> _mockValidator;
        private readonly ValidationMarketDataServiceDecorator<IMarketDataEntity> _decorator;
        private readonly Mock<IMarketDataEntity> _mockMarketData;

        public ValidationDecoratorTests()
        {
            _mockMarketDataService = new Mock<IMarketDataService<IMarketDataEntity>>();
            _mockValidator = new Mock<IValidator<IMarketDataEntity>>();
            _mockMarketData = new Mock<IMarketDataEntity>();

            _decorator = new ValidationMarketDataServiceDecorator<IMarketDataEntity>(
                _mockMarketDataService.Object,
                _mockValidator.Object);
        }

        [Fact]
        public async Task PublishMarketDataAsync_WhenValidationSucceeds_ShouldCallService()
        {
            // Arrange
            var marketData = _mockMarketData.Object;
            var expectedResult = "success";

            _mockValidator
                .Setup(v => v.ValidateAsync(marketData, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidationResult.Success());

            _mockMarketDataService
                .Setup(s => s.PublishMarketDataAsync(marketData))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _decorator.PublishMarketDataAsync(marketData);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockMarketDataService.Verify(s => s.PublishMarketDataAsync(marketData), Times.Once);
        }

        [Fact]
        public async Task PublishMarketDataAsync_WhenValidationFails_ShouldThrowException()
        {
            // Arrange
            var marketData = _mockMarketData.Object;
            var validationErrors = new[] { new ValidationError("Property", "Error message") };

            _mockValidator
                .Setup(v => v.ValidateAsync(marketData, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidationResult.Failure(validationErrors));
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(
                async () => await _decorator.PublishMarketDataAsync(marketData));

            _mockMarketDataService.Verify(s => s.PublishMarketDataAsync(marketData), Times.Never);
        }
    }
}