using System;
using System.Collections.Generic;
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
        public class TestBaseEntity : IMarketDataEntity
        {
            public string Id => $"{DataType}.{AssetClass}/{AssetId}/{AsOfDate:yyyyMMdd}/{DocumentType}/{Version}";
            public string SchemaVersion { get; set; } = "1.0";
            public int? Version { get; set; } = 1;
            public string AssetId { get; set; } = "TEST";
            public string AssetClass { get; set; } = "test";
            public string DataType { get; set; } = "testdata";
            public string Region { get; set; } = "global";

            // Fix: Change to IReadOnlyList<string> to match the interface
            private readonly List<string> _tags = new List<string>();
            public IReadOnlyList<string> Tags => _tags;

            public string DocumentType { get; set; } = "test";
            public DateTimeOffset CreateTimestamp => DateTimeOffset.UtcNow;
            public DateOnly AsOfDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly? AsOfTime { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
        }

        public class TestDerivedEntity : TestBaseEntity { }

        // Additional derived classes for testing multiple validators
        public class TestDerivedEntity2 : TestBaseEntity { }
        public class TestDerivedFromDerivedEntity : TestDerivedEntity { }

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
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_WithNullEntity_ThrowsArgumentNullException()
        {
            // Arrange
            var compositeValidator = new CompositeValidator<TestBaseEntity>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                compositeValidator.ValidateAsync(null!));
        }

        [Fact]
        public async Task ValidateAsync_WithBaseEntityAndDerivedValidator_ReturnsSuccess()
        {
            // Arrange
            var compositeValidator = new CompositeValidator<TestBaseEntity>();
            var derivedEntityValidator = new Mock<IValidator<TestDerivedEntity>>();

            derivedEntityValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestDerivedEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidationResult.Success());

            compositeValidator.RegisterValidator(derivedEntityValidator.Object);

            // Create a base entity (not derived)
            var baseEntity = new TestBaseEntity();

            // Act
            var result = await compositeValidator.ValidateAsync(baseEntity);

            // Assert
            // The validator should return success since no validator is registered for TestBaseEntity
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);

            // The derivedEntityValidator should not be called because baseEntity is not a TestDerivedEntity
            derivedEntityValidator.Verify(
                v => v.ValidateAsync(It.IsAny<TestDerivedEntity>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ValidateAsync_WithFailingValidator_ReturnsFailure()
        {
            // Arrange
            var compositeValidator = new CompositeValidator<TestBaseEntity>();
            var derivedEntityValidator = new Mock<IValidator<TestDerivedEntity>>();

            var expectedErrors = new[] { new ValidationError("TestProperty", "Error message") };
            var failureResult = ValidationResult.Failure(expectedErrors);

            derivedEntityValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestDerivedEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failureResult);

            compositeValidator.RegisterValidator(derivedEntityValidator.Object);

            var entity = new TestDerivedEntity();

            // Act
            var result = await compositeValidator.ValidateAsync(entity);

            // Assert
            Assert.False(result.IsValid);
            Assert.Same(failureResult, result);
            Assert.Single(result.Errors);
            Assert.Equal("TestProperty", result.Errors[0].PropertyName);
            Assert.Equal("Error message", result.Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task ValidateAsync_WithMultipleRegisteredValidators_CallsCorrectValidator()
        {
            // Arrange
            var compositeValidator = new CompositeValidator<TestBaseEntity>();

            // Set up validators for different entity types
            var derivedEntityValidator = new Mock<IValidator<TestDerivedEntity>>();
            var derivedEntity2Validator = new Mock<IValidator<TestDerivedEntity2>>();

            var result1 = ValidationResult.Success();
            var result2 = ValidationResult.Failure("Test failure");

            derivedEntityValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestDerivedEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result1);

            derivedEntity2Validator
                .Setup(v => v.ValidateAsync(It.IsAny<TestDerivedEntity2>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result2);

            // Register both validators
            compositeValidator.RegisterValidator(derivedEntityValidator.Object);
            compositeValidator.RegisterValidator(derivedEntity2Validator.Object);

            // Create entities of different types
            var entity1 = new TestDerivedEntity();
            var entity2 = new TestDerivedEntity2();

            // Act
            var actualResult1 = await compositeValidator.ValidateAsync(entity1);
            var actualResult2 = await compositeValidator.ValidateAsync(entity2);

            // Assert
            Assert.Same(result1, actualResult1);
            Assert.Same(result2, actualResult2);

            // Fixed: Use generic It.IsAny<Type>() to avoid type conversion issues
            derivedEntityValidator.Verify(v => v.ValidateAsync(It.IsAny<TestDerivedEntity>(), It.IsAny<CancellationToken>()), Times.Once);
            derivedEntity2Validator.Verify(v => v.ValidateAsync(It.IsAny<TestDerivedEntity2>(), It.IsAny<CancellationToken>()), Times.Once);

            // No need to verify that validators weren't called with incorrect types - this is implied
        }

        [Fact]
        public async Task ValidateAsync_WithSubclassOfRegisteredType_UsesMostSpecificValidator()
        {
            // Arrange
            var compositeValidator = new CompositeValidator<TestBaseEntity>();

            // Set up validators for different entity types
            var derivedEntityValidator = new Mock<IValidator<TestDerivedEntity>>();
            var derivedFromDerivedValidator = new Mock<IValidator<TestDerivedFromDerivedEntity>>();

            var baseResult = ValidationResult.Failure("Base validator called");
            var specificResult = ValidationResult.Success();

            derivedEntityValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestDerivedEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(baseResult);

            derivedFromDerivedValidator
                .Setup(v => v.ValidateAsync(It.IsAny<TestDerivedFromDerivedEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(specificResult);

            // Register both validators
            compositeValidator.RegisterValidator(derivedEntityValidator.Object);
            compositeValidator.RegisterValidator(derivedFromDerivedValidator.Object);

            // Create a deeply derived entity
            var entity = new TestDerivedFromDerivedEntity();

            // Act
            var result = await compositeValidator.ValidateAsync(entity);

            // Assert - should use the most specific validator
            Assert.Same(specificResult, result);

            derivedFromDerivedValidator.Verify(
                v => v.ValidateAsync(entity, It.IsAny<CancellationToken>()),
                Times.Once);

            // The base validator should not be called
            derivedEntityValidator.Verify(
                v => v.ValidateAsync(It.IsAny<TestDerivedEntity>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ValidateAsync_WithValidatorThatThrows_PropagatesException()
        {
            // Arrange
            var compositeValidator = new CompositeValidator<TestBaseEntity>();
            var validator = new Mock<IValidator<TestDerivedEntity>>();

            var expectedException = new InvalidOperationException("Validation failed");

            validator
                .Setup(v => v.ValidateAsync(It.IsAny<TestDerivedEntity>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            compositeValidator.RegisterValidator(validator.Object);

            var entity = new TestDerivedEntity();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await compositeValidator.ValidateAsync(entity));

            Assert.Same(expectedException, exception);
        }
    }
}