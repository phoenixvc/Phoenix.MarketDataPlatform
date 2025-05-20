using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phoenix.MarketData.Core.Events;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Repositories;

namespace Phoenix.MarketData.Application.Services
{
    public interface IMarketDataService<T> where T : IMarketDataEntity
    {
        Task<string> PublishMarketDataAsync(T marketData);
        Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType);
        Task<IEnumerable<T>> QueryMarketDataAsync(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null);
    }

    public class MarketDataService<T> : IMarketDataService<T> where T : class, IMarketDataEntity
    {
        private readonly CosmosRepository<T> _repository;
        private readonly IMarketDataEventPublisher _eventPublisher;
        private readonly IMarketDataValidator _validator;

        public MarketDataService(
            CosmosRepository<T> repository,
            IMarketDataEventPublisher eventPublisher,
            IMarketDataValidator validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<string> PublishMarketDataAsync(T marketData)
        {
            if (marketData == null)
                throw new ArgumentNullException(nameof(marketData));
            _validator.ValidateMarketData(marketData);
            var id = await _repository.AddAsync(marketData);
            await _eventPublisher.PublishDataChangedEventAsync(marketData);
            return id is IEntity entity ? entity.Id : string.Empty;
        }

        public async Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType)
        {
            var result = await _repository.GetLatestMarketDataAsync(
                dataType, assetClass, assetId, region, asOfDate, documentType);
            return result;
        }

        public async Task<IEnumerable<T>> QueryMarketDataAsync(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            return await _repository.QueryByRangeAsync(
                dataType, assetClass, assetId, fromDate, toDate);
        }
    }
}