using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Events;
using Phoenix.MarketData.Domain.Validation;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Repositories;

namespace Phoenix.MarketData.Application.Services
{
    public interface IMarketDataService<T> where T : IMarketDataEntity
    {
        Task<string> PublishMarketDataAsync(T marketData);
        Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType,
            CancellationToken cancellationToken = default);  // Added cancellationToken parameter
        Task<IEnumerable<T>> QueryMarketDataAsync(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null,
            CancellationToken cancellationToken = default);
    }

    public class MarketDataService<T> : IMarketDataService<T> where T : class, IMarketDataEntity
    {
        private readonly CosmosRepository<T> _repository;
        private readonly IMarketDataEventPublisher _eventPublisher;
        private readonly IValidator<T> _validator;

        public MarketDataService(
            CosmosRepository<T> repository,
            IMarketDataEventPublisher eventPublisher,
            IValidator<T> validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<string> PublishMarketDataAsync(T marketData)
        {
            if (marketData == null)
                throw new ArgumentNullException(nameof(marketData));
            var validationResult = await _validator.ValidateAsync(marketData);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var id = await _repository.AddAsync(marketData);
            await _eventPublisher.PublishDataChangedEventAsync(marketData);

            // Return the repository's ID directly instead of re-deriving it
            return id?.ToString() ?? string.Empty;
        }

        public async Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType,
            CancellationToken cancellationToken = default)  // Added cancellationToken parameter
        {
            var result = await _repository.GetLatestMarketDataAsync(
                dataType, assetClass, assetId, region, asOfDate, documentType,
                cancellationToken);  // Pass cancellationToken to repository
            return result;
        }

        public async Task<IEnumerable<T>> QueryMarketDataAsync(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            return await _repository.QueryByRangeAsync(
                dataType, assetClass, assetId, fromDate, toDate,
                cancellationToken);  // Pass cancellationToken to repository
        }
    }
}