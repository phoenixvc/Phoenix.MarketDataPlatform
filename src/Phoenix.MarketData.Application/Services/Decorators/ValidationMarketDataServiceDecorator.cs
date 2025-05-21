using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Application.Services;
namespace Phoenix.MarketData.Application.Services.Decorators
{
    /// <summary>
    /// A decorator for IMarketDataService that adds validation before operations
    /// </summary>
    public class ValidationMarketDataServiceDecorator<T> : ServiceValidationDecorator<IMarketDataService<T>>, IMarketDataService<T>
        where T : IMarketDataEntity
    {
        private readonly IValidator<T> _marketDataValidator;

        public ValidationMarketDataServiceDecorator(
            IMarketDataService<T> marketDataService,
            IValidator<T> marketDataValidator)
            : base(marketDataService)
        {
            _marketDataValidator = marketDataValidator ?? throw new ArgumentNullException(nameof(marketDataValidator));
        }

        public async Task<string> PublishMarketDataAsync(T marketData)
        {
            // Validate before publishing
            await ValidateAsync(marketData, _marketDataValidator);

            // Delegate to the decorated service
            return await DecoratedService.PublishMarketDataAsync(marketData);
        }

        public Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType)
        {
            // No validation needed for read operations
            return DecoratedService.GetLatestMarketDataAsync(
                dataType, assetClass, assetId, region, asOfDate, documentType);
        }

        public Task<IEnumerable<T>> QueryMarketDataAsync(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            // No validation needed for read operations
            return DecoratedService.QueryMarketDataAsync(
                dataType, assetClass, assetId, fromDate, toDate);
        }
    }
}