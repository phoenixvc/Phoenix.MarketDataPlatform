using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
namespace Phoenix.MarketData.Infrastructure.Validation
{
    public class FxSpotPriceDataValidator : IValidator<FxSpotPriceData>
    {
        public Task<ValidationResult> ValidateAsync(FxSpotPriceData data, CancellationToken cancellationToken = default)
        {
            var errors = new List<ValidationError>();

            // Add validation logic here
            if (string.IsNullOrEmpty(data.AssetId))
            {
                errors.Add(new ValidationError("AssetId", "Asset ID is required"));
            }

            if (data.Price <= 0)
            {
                errors.Add(new ValidationError("Price", "Price must be greater than zero."));
            }

            if (string.IsNullOrWhiteSpace(data.Currency))
            {
                errors.Add(new ValidationError("Currency", "Currency cannot be null or empty."));
            }

            // Add more validation rules as needed

            return Task.FromResult(errors.Count > 0
                ? ValidationResult.Failure(errors)  // Using the method from the Core ValidationResult
                : ValidationResult.Success());      // Fixed: Using method call, not property access
        }
    }
}