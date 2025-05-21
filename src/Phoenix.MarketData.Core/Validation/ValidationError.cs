using System.Collections.Generic;
using System.Linq;

namespace Phoenix.MarketData.Core.Validation;

public class ValidationError
{
    public string? PropertyName { get; set; }
    public string ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public string? Source { get; set; }

    public override string ToString()
        => string.IsNullOrWhiteSpace(PropertyName)
            ? ErrorMessage
                : $"{PropertyName}: {ErrorMessage}";

    public ValidationError()
    {
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Creates a validation error with just an error message
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    public ValidationError(string errorMessage)
    {
        ErrorMessage = errorMessage ?? string.Empty;
    }

    /// <summary>
    /// Creates a validation error with property name and error message
    /// </summary>
    /// <param name="propertyName">Name of the property with the error</param>
    /// <param name="errorMessage">The error message</param>
    public ValidationError(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    /// <summary>
    /// Creates a validation error with property name, error message, and error code
    /// </summary>
    /// <param name="propertyName">Name of the property with the error</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="errorCode">Error code for the validation error</param>
    public ValidationError(string propertyName, string errorMessage, string errorCode)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage ?? string.Empty;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a validation error with all details
    /// </summary>
    /// <param name="propertyName">Name of the property with the error</param>
    /// <param name="errorMessage">The error message</param>
    /// <param name="errorCode">Error code for the validation error</param>
    /// <param name="source">Source of the validation error</param>
    public ValidationError(string propertyName, string errorMessage, string errorCode, string source)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage ?? string.Empty;
        ErrorCode = errorCode;
        Source = source;
    }
}