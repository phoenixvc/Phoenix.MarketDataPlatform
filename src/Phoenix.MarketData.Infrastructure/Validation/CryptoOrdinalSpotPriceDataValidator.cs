using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Validation;

namespace Phoenix.MarketData.Infrastructure.Validation;

public class CryptoOrdinalSpotPriceDataValidator : IValidator<CryptoOrdinalSpotPriceData>
{
    public void Validate(CryptoOrdinalSpotPriceData data)
    {
        if (data.Price <= 0)
            throw new ValidationException("Price must be greater than zero.");

        if (data.InscriptionNumber < 0)
            throw new ValidationException("InscriptionNumber cannot be negative.");

        if (string.IsNullOrWhiteSpace(data.InscriptionId))
            throw new ValidationException("InscriptionId cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(data.Currency))
            throw new ValidationException("Currency cannot be null or empty.");

        // Add any other crypto-specific checks here
    }
}
