using FluentValidation.Results;
using System;
using System.Linq;

namespace Phoenix.MarketData.Domain.Validation;

/// <summary>
/// Extension methods for FluentValidation results
/// </summary>
public static class FluentValidationExtensions
{
    /// <summary>
    /// Converts a FluentValidation validation result to the application's ValidationResult format
    /// </summary>
    /// <param name="result">The FluentValidation result to convert</param>
    /// <returns>A ValidationResult instance containing any validation errors</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null</exception>
    public static ValidationResult ToValidationResult(this FluentValidation.Results.ValidationResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result), "Validation result cannot be null");

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