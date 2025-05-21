using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Core.Validation;

namespace Phoenix.MarketData.Infrastructure.Validation
{
    public class FxSpotPriceDataValidator : IValidator<FxSpotPriceData>
    {
        public Task<ValidationResult> ValidateAsync(FxSpotPriceData data, CancellationToken cancellationToken = default)
        {
            // Add proper null check that throws ArgumentNullException
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var errors = new List<ValidationError>();
            // Validate AssetId
            if (string.IsNullOrEmpty(data.AssetId) || !data.AssetId.Contains('/'))
            {
                errors.Add(new ValidationError("AssetId", "Asset ID is required and must be in the format XXX/YYY"));
            }

            // Validate AssetClass
            if (string.IsNullOrWhiteSpace(data.AssetClass) || data.AssetClass.ToLower() != "fx")
            {
                errors.Add(new ValidationError("AssetClass", "Asset class must be 'fx'"));
            }

            // Validate Price
            if (data.Price <= 0)
            {
                errors.Add(new ValidationError("Price", "Price must be greater than zero."));
            }

            // Validate Currency
            if (string.IsNullOrWhiteSpace(data.Currency))
            {
                errors.Add(new ValidationError("Currency", "Currency cannot be null or empty."));
            }
            // Add more validation rules as needed

            return Task.FromResult(errors.Count > 0
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success());
        }
    }
}