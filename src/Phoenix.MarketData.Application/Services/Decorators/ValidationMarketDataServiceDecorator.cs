using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Validation;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Application.Services.Decorators
{
    public class ValidationMarketDataServiceDecorator<T> : ServiceValidationDecorator<IMarketDataService<T>>
        where T : class, IMarketDataEntity
    {
        private readonly IValidator<T> _validator;

        public ValidationMarketDataServiceDecorator(IMarketDataService<T> decorated, IValidator<T> validator)
            : base(decorated)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<string> PublishMarketDataAsync(T marketData)
        {
            // Validate market data before delegating to decorated service
            var validationResult = await _validator.ValidateAsync(marketData).ConfigureAwait(false);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            return await DecoratedService.PublishMarketDataAsync(marketData).ConfigureAwait(false);
        }

        public async Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType)
        {
            // No validation needed for read operations
            return await DecoratedService.GetLatestMarketDataAsync(dataType, assetClass, assetId, region, asOfDate, documentType).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> QueryMarketDataAsync(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            // No validation needed for read operations
            return await DecoratedService.QueryMarketDataAsync(dataType, assetClass, assetId, fromDate, toDate, cancellationToken).ConfigureAwait(false);
        }
    }
}