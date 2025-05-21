using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Validation;
using Xunit;

namespace Phoenix.MarketData.Infrastructure.Tests.Validation
{
    public class CompositeValidatorTests
    {
        // Test entity classes
        public class TestBaseEntity : IMarketDataEntity { }
        public class TestDerivedEntity : TestBaseEntity { }

        [Fact]
        public async Task ValidateAsync_WithRegisteredValidator_ReturnsValidationResult()
        {
            // Arrange
            var compositeValidator = new CompositeValidator<TestBaseEntity>();
            var derivedEntityValidator = new Mock<IValidator<TestDerivedEntity>>();

            var expectedResult = ValidationResult.Success();
            derivedEntityValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestDerivedEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            compositeValidator.RegisterValidator(derivedEntityValidator.Object);

            var entity = new TestDerivedEntity();

            // Act
            var result = await compositeValidator.ValidateAsync(entity);

            // Assert
            Assert.Same(expectedResult, result);
            derivedEntityValidator.Verify(
                v => v.ValidateAsync(entity, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ValidateAsync_WithNoRegisteredValidator_ReturnsSuccess()
        {
            // Arrange
            var compositeValidator = new CompositeValidator<TestBaseEntity>();
            var entity = new TestDerivedEntity();

            // Act
            var result = await compositeValidator.ValidateAsync(entity);

            // Assert
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WithNullEntity_ThrowsArgumentNullException()
        {
            // Arrange
            var compositeValidator = new CompositeValidator<TestBaseEntity>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                compositeValidator.ValidateAsync(null));
        }

        [Fact]
        public async Task ValidateAsync_WithWrongEntityType_ThrowsInvalidOperationException()
        {
            // Arrange
            var compositeValidator = new CompositeValidator<TestBaseEntity>();
            var specificValidator = new Mock<IValidator<TestDerivedEntity>>();

            specificValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestDerivedEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidationResult.Success());

            compositeValidator.RegisterValidator(specificValidator.Object);

            // Create a mock that is TestBaseEntity but not TestDerivedEntity
            var mockBaseEntity = new Mock<TestBaseEntity>();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                compositeValidator.ValidateAsync(mockBaseEntity.Object));
        }
    }
}