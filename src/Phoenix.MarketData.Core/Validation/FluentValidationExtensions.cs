using FluentValidation.Results;
using System.Linq;

namespace Phoenix.MarketData.Core.Validation;

public static class FluentValidationExtensions
{
    public static ValidationResult ToValidationResult(this FluentValidation.Results.ValidationResult result)
    {
        var errors = result.Errors.Select(e => new ValidationError
        {
            PropertyName = e.PropertyName,
            ErrorCode = e.ErrorCode,
            ErrorMessage = e.ErrorMessage,
            Source = "FluentValidation"
        });

        return errors.Any()
            ? ValidationResult.Failure(errors)
            : ValidationResult.Success();
    }
}
