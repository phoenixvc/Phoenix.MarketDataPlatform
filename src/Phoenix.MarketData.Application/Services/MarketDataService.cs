using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Events;
using Phoenix.MarketData.Domain.Models.Interfaces;
using Phoenix.MarketData.Domain.Repositories;

namespace Phoenix.MarketData.Application.Services
{
    public class MarketDataService : IMarketDataService
    {
        private readonly IMarketDataRepository _repository;
        private readonly IMarketDataEventPublisher _eventPublisher;
        private readonly IMarketDataValidator _validator;

        public MarketDataService(
            IMarketDataRepository repository,
            IMarketDataEventPublisher eventPublisher,
            IMarketDataValidator validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        // Push model - data is pushed to the system
        public async Task<string> PublishMarketDataAsync<T>(T marketData) where T : IMarketData
        {
            // Validate the data
            _validator.ValidateMarketData(marketData);
            
            // Save to repository
            var id = await _repository.SaveMarketDataAsync(marketData);
            
            // Publish event
            await _eventPublisher.PublishDataChangedEventAsync(marketData);
            
            return id;
        }

        // Pull model - data is retrieved from the system
        public async Task<T?> GetLatestMarketDataAsync<T>(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType) where T : IMarketData
        {
            var result = await _repository.GetMarketDataByLatestVersionAsync<T>(
                dataType, assetClass, assetId, region, asOfDate, documentType);
                
            return result.Result;
        }
        
        public async Task<IEnumerable<T>> QueryMarketDataAsync<T>(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null) where T : IMarketData
        {
            return await _repository.QueryMarketDataAsync<T>(
                dataType, assetClass, assetId, fromDate, toDate);
        }
    }
}