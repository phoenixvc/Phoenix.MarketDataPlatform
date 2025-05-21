using System.Collections.Generic;
using System.Linq;

namespace Phoenix.MarketData.Core.Validation
{
    public class ValidationResult
    {
        private List<ValidationError> _errors;

        /// <summary>
        /// Indicates whether validation passed with no errors
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Collection of validation errors
        /// </summary>
        public IReadOnlyList<ValidationError> Errors => _errors;

        /// <summary>
        /// Creates a validation result with optional errors
        /// </summary>
        /// <param name="errors">Optional collection of validation errors</param>
        public ValidationResult(IEnumerable<ValidationError>? errors = null)
        {
            _errors = errors?.ToList() ?? new List<ValidationError>();
        }

        /// <summary>
        /// Creates a successful validation result with no errors
        /// </summary>
        public static ValidationResult Success() => new ValidationResult();

        /// <summary>
        /// Creates a failed validation result with the specified errors
        /// </summary>
        public static ValidationResult Failure(IEnumerable<ValidationError> errors) => new ValidationResult(errors);

        /// <summary>
        /// Creates a failed validation result with a single error message
        /// </summary>
        public static ValidationResult Failure(string errorMessage)
            => new ValidationResult(new[] { new ValidationError(errorMessage) });

        /// <summary>
        /// Creates a failed validation result with property name and error message
        /// </summary>
        public static ValidationResult Failure(string propertyName, string errorMessage)
            => new ValidationResult(new[] { new ValidationError(propertyName, errorMessage) });

        /// <summary>
        /// Creates a failed validation result with property name, error message, and error code
        /// </summary>
        public static ValidationResult Failure(string propertyName, string errorMessage, string errorCode)
            => new ValidationResult(new[] { new ValidationError(propertyName, errorMessage, errorCode) });

        /// <summary>
        /// Creates a failed validation result with property name, error message, error code, and source
        /// </summary>
        public static ValidationResult Failure(string propertyName, string errorMessage, string errorCode, string source)
            => new ValidationResult(new[]
            {
                new ValidationError(propertyName, errorMessage, errorCode, source)
            });

        public override string ToString()
            => IsValid ? "Valid" : string.Join("; ", Errors.Select(e => e.ToString()));

        /// <summary>
        /// Returns a flat string of all error messages (not just the "ToString" of each error).
        /// </summary>
        public string ErrorMessage
            => string.Join("; ", Errors.Select(e => e.ErrorMessage));

        /// <summary>
        /// Adds an error to the validation result
        /// </summary>
        public void AddError(ValidationError error)
        {
            if (error != null)
            {
                _errors.Add(error);
            }
        }

        /// <summary>
        /// Adds multiple errors to the validation result
        /// </summary>
        public void AddErrors(IEnumerable<ValidationError> errors)
        {
            if (errors != null)
            {
                _errors.AddRange(errors);
            }
        }
    }
}