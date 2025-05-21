using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Core.Validation;
namespace Phoenix.MarketData.Infrastructure.Validation
{
    public class CryptoOrdinalSpotPriceDataValidator : IValidator<CryptoOrdinalSpotPriceData>
    {
        public Task<ValidationResult> ValidateAsync(CryptoOrdinalSpotPriceData data, CancellationToken cancellationToken = default)
        {
            var errors = new List<ValidationError>();
            if (data.Price <= 0)
                errors.Add(new ValidationError("Price", "Price must be greater than zero."));
            if (data.InscriptionNumber < 0)
                errors.Add(new ValidationError("InscriptionNumber", "InscriptionNumber cannot be negative."));
            if (string.IsNullOrWhiteSpace(data.InscriptionId))
                errors.Add(new ValidationError("InscriptionId", "InscriptionId cannot be null or empty."));
            if (string.IsNullOrWhiteSpace(data.Currency))
                errors.Add(new ValidationError("Currency", "Currency cannot be null or empty."));
            return Task.FromResult(errors.Count > 0
                ? ValidationResult.Failure(errors)
                : ValidationResult.Success());
        }
    }
}
