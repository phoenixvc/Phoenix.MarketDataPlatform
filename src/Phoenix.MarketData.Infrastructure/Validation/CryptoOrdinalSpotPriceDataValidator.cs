using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Phoenix.MarketData.Infrastructure.Validation
{
    public class CryptoOrdinalSpotPriceDataValidator : IValidator<CryptoOrdinalSpotPriceData>
    {
        public Task<ValidationResult> ValidateAsync(CryptoOrdinalSpotPriceData data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            var errors = new List<ValidationError>();

            // Validate AssetId
            if (string.IsNullOrWhiteSpace(data.AssetId))
            {
                errors.Add(new ValidationError("AssetId", "Asset ID is required"));
            }
            else if (!data.AssetId.Contains('/'))
            {
                errors.Add(new ValidationError("AssetId", "Asset ID format is invalid, expected format: XXX/YYY"));
            }

            // Validate AssetClass
            if (string.IsNullOrWhiteSpace(data.AssetClass) || data.AssetClass.ToLower() != "crypto")
            {
                errors.Add(new ValidationError("AssetClass", "Asset class must be 'crypto'"));
            }

            // Validate Price
            if (data.Price <= 0)
            {
                errors.Add(new ValidationError("Price", "Price must be greater than zero"));
            }

            // Validate Required Ordinal Properties
            if (string.IsNullOrWhiteSpace(data.InscriptionId))
            {
                errors.Add(new ValidationError("InscriptionId", "Inscription ID is required"));
            }

            var result = new ValidationResult(errors);
            return Task.FromResult(result);
        }
    }
}
