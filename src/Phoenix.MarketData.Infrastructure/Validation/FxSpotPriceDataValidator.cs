using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Infrastructure.Validation;

namespace Phoenix.MarketData.Infrastructure.Validation;

public class FxSpotPriceDataValidator : IValidator<FxSpotPriceData>
{
    public void Validate(FxSpotPriceData data)
    {
        if (data.Price <= 0)
            throw new ValidationException("Price must be greater than zero.");
        if (string.IsNullOrWhiteSpace(data.Currency))
            throw new ValidationException("Currency cannot be null or empty.");
        // Add type-specific logic here
    }
}
