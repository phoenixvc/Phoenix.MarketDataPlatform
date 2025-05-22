using System;
using System.Collections.Generic;
using System.Linq;

namespace Phoenix.MarketData.Core.Validation
{
    /// <summary>
    /// Represents the outcome of a validation operation
    /// </summary>
    public class ValidationResult
    {
        private readonly List<ValidationError> _errors;

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
        /// <returns>A validation result with no errors</returns>
        public static ValidationResult Success() => new ValidationResult();

        /// <summary>
        /// Creates a failed validation result with a single validation error
        /// </summary>
        /// <param name="error">The validation error</param>
        /// <returns>A validation result with the error</returns>
        public static ValidationResult Failure(ValidationError error) =>
            error == null ?
                throw new ArgumentNullException(nameof(error)) :
                new ValidationResult(new[] { error });

        /// <summary>
        /// Creates a failed validation result with the specified errors
        /// </summary>
        /// <param name="errors">Collection of validation errors</param>
        /// <returns>A validation result with the errors</returns>
        public static ValidationResult Failure(IEnumerable<ValidationError> errors) =>
            new ValidationResult(errors);

        /// <summary>
        /// Creates a failed validation result with a single error message
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <returns>A validation result with the error</returns>
        public static ValidationResult Failure(string errorMessage) =>
            new ValidationResult(new[] { new ValidationError(errorMessage) });

        /// <summary>
        /// Creates a failed validation result with property name and error message
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <param name="errorMessage">The error message</param>
        /// <returns>A validation result with the error</returns>
        public static ValidationResult Failure(string propertyName, string errorMessage) =>
            new ValidationResult(new[] { new ValidationError(propertyName, errorMessage) });

        /// <summary>
        /// Creates a failed validation result with property name, error message, and error code
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <returns>A validation result with the error</returns>
        public static ValidationResult Failure(string propertyName, string errorMessage, string errorCode) =>
            new ValidationResult(new[] { new ValidationError(propertyName, errorMessage, errorCode) });

        /// <summary>
        /// Creates a failed validation result with property name, error message, error code, and source
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="source">The source of the error</param>
        /// <returns>A validation result with the error</returns>
        public static ValidationResult Failure(string propertyName, string errorMessage, string errorCode, string source) =>
            new ValidationResult(new[] { new ValidationError(propertyName, errorMessage, errorCode, source) });

        /// <summary>
        /// Combines multiple validation results into a single result containing all errors
        /// </summary>
        /// <param name="results">The validation results to combine</param>
        /// <returns>A new validation result containing all errors from the input results</returns>
        public static ValidationResult Combine(params ValidationResult[] results)
        {
            if (results == null || results.Length == 0)
                return Success();

            var allErrors = results
                .Where(r => r != null)
                .SelectMany(r => r.Errors)
                .ToList();

            return new ValidationResult(allErrors);
        }

        /// <summary>
        /// Creates a new validation result by adding the specified error to this result's errors
        /// </summary>
        /// <param name="error">The error to add</param>
        /// <returns>A new validation result containing all existing errors plus the new error</returns>
        public ValidationResult WithError(ValidationError error)
        {
            if (error == null)
                return this;

            var newErrors = new List<ValidationError>(_errors) { error };
            return new ValidationResult(newErrors);
        }

        /// <summary>
        /// Creates a new validation result by adding the specified errors to this result's errors
        /// </summary>
        /// <param name="errors">The errors to add</param>
        /// <returns>A new validation result containing all existing errors plus the new errors</returns>
        public ValidationResult WithErrors(IEnumerable<ValidationError> errors)
        {
            if (errors == null || !errors.Any())
                return this;

            var newErrors = new List<ValidationError>(_errors);
            newErrors.AddRange(errors);
            return new ValidationResult(newErrors);
        }

        /// <summary>
        /// Returns a string representation of the validation result
        /// </summary>
        /// <returns>A string representation</returns>
        public override string ToString() =>
            IsValid ? "Valid" : string.Join("; ", Errors.Select(e => e.ToString()));

        /// <summary>
        /// Returns a flat string of all error messages
        /// </summary>
        public string ErrorMessage =>
            string.Join("; ", Errors.Select(e => e.ErrorMessage));
    }
}