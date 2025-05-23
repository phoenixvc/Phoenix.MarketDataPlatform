using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Domain.Repositories;
using Phoenix.MarketData.Application.Services;

namespace Phoenix.MarketData.Application.Tests.Services
{
    public class MarketDataServiceTests
    {
        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            var validator = new Mock<IValidator<TestEntity>>().Object;
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new MarketDataService<TestEntity>(null, validator));
        }
        
        [Fact]
        public void Constructor_WithNullValidator_ThrowsArgumentNullException()
        {
            // Arrange
            var repository = new Mock<IMarketDataRepository<TestEntity>>().Object;
            
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                new MarketDataService<TestEntity>(repository, null));
        }
        
        [Fact]
        public async Task GetByIdAsync_CallsRepository()
        {
            // Arrange
            var mockRepository = new Mock<IMarketDataRepository<TestEntity>>();
            var mockValidator = new Mock<IValidator<TestEntity>>();
            var service = new MarketDataService<TestEntity>(mockRepository.Object, mockValidator.Object);
            
            var entity = new TestEntity();
            var id = "test-id";
            
            mockRepository
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);
                
            // Act
            var result = await service.GetByIdAsync(id);
            
            // Assert
            Assert.Same(entity, result);
            mockRepository.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task SaveAsync_ValidatesEntityBeforeSaving()
        {
            // Arrange
            var mockRepository = new Mock<IMarketDataRepository<TestEntity>>();
            var mockValidator = new Mock<IValidator<TestEntity>>();
            var service = new MarketDataService<TestEntity>(mockRepository.Object, mockValidator.Object);
            
            var entity = new TestEntity();
            var validationResult = ValidationResult.Success();
            
            mockValidator
                .Setup(v => v.ValidateAsync(entity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
                
            mockRepository
                .Setup(r => r.SaveAsync(entity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);
                
            // Act
            var result = await service.SaveAsync(entity);
            
            // Assert
            Assert.Same(entity, result);
            mockValidator.Verify(v => v.ValidateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
            mockRepository.Verify(r => r.SaveAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task SaveAsync_WithInvalidEntity_ThrowsValidationException()
        {
            // Arrange
            var mockRepository = new Mock<IMarketDataRepository<TestEntity>>();
            var mockValidator = new Mock<IValidator<TestEntity>>();
            var service = new MarketDataService<TestEntity>(mockRepository.Object, mockValidator.Object);
            
            var entity = new TestEntity();
            var errors = new[] { new ValidationError("Property", "Error message") };
            var validationResult = ValidationResult.Failure(errors);
            
            mockValidator
                .Setup(v => v.ValidateAsync(entity, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);
                
            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => 
                service.SaveAsync(entity));
                
            // Repository should not be called if validation fails
            mockRepository.Verify(r => r.SaveAsync(entity, It.IsAny<CancellationToken>()), Times.Never);
        }
        
        // Test helper class
        private class TestEntity : IMarketDataEntity
        {
            public string Id => $"{DataType}.{AssetClass}/{AssetId}/{AsOfDate:yyyyMMdd}/{DocumentType}/{Version}";
            public string SchemaVersion { get; set; } = "1.0";
            public int? Version { get; set; } = 1;
            public string AssetId { get; set; } = "TEST";
            public string AssetClass { get; set; } = "test";
            public string DataType { get; set; } = "testdata";
            public string Region { get; set; } = "global";
            public List<string> Tags { get; set; } = new List<string>();
            public string DocumentType { get; set; } = "test";
            public DateTimeOffset CreateTimestamp => DateTimeOffset.UtcNow;
            public DateOnly AsOfDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly? AsOfTime { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
        }
    }
}