using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Schema;

namespace Phoenix.MarketData.Core.Validation;

/// <summary>
/// Extension methods for Json.Schema validation results
/// </summary>
public static class SchemaValidationExtensions
{
    /// <summary>
    /// Source identifier for JSON schema validation errors
    /// </summary>
    private const string SchemaValidationSource = "JsonSchema.Net";

    /// <summary>
    /// Converts Json.Schema validation results to the application's ValidationResult format
    /// </summary>
    /// <param name="results">The schema validation results to convert</param>
    /// <returns>A ValidationResult instance containing any validation errors</returns>
    /// <exception cref="ArgumentNullException">Thrown when results is null</exception>
    public static ValidationResult ToValidationResult(this ValidationResults results)
    {
        if (results == null)
            throw new ArgumentNullException(nameof(results), "Validation results cannot be null");

        var errors = CollectSchemaErrors(results).ToList();
        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success();
    }

    /// <summary>
    /// Recursively collects all schema validation errors from the ValidationResults structure and yields them as a flat sequence of <see cref="ValidationError"/> objects.
    /// </summary>
    /// <param name="results">The ValidationResults from which to collect errors.</param>
    /// <param name="parentPath">The path to append to the instance location of the result to form the full property name.</param>
    /// <returns>A sequence of all validation errors found in the ValidationResults structure.</returns>
    private static IEnumerable<ValidationError> CollectSchemaErrors(ValidationResults results, string parentPath = "")
    {
        var location = results.InstanceLocation?.ToString() ?? string.Empty;
        var path = string.IsNullOrEmpty(parentPath)
            ? location
            : string.IsNullOrEmpty(location)
                ? parentPath
                : $"{parentPath}{location}";

        // Ensure path is never empty - use "root" as default path if both parentPath and location are empty
        path = string.IsNullOrEmpty(path) ? "root" : path;

        if (!results.IsValid && !string.IsNullOrWhiteSpace(results.Message))
        {
            yield return new ValidationError
            {
                PropertyName = path,
                ErrorMessage = results.Message,
                Source = SchemaValidationSource
            };
        }

        if (results.NestedResults != null)
        {
            foreach (var nested in results.NestedResults)
            {
                foreach (var error in CollectSchemaErrors(nested, path))
                    yield return error;
            }
        }
    }
}