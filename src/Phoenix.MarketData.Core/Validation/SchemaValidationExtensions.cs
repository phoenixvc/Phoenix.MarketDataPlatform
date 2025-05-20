using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.Schema;
using Phoenix.MarketData.Core.Validation; 

namespace Phoenix.MarketData.Core.Validation;

public static class SchemaValidationExtensions
{
    public static ValidationResult ToValidationResult(this ValidationResults results)
    {
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
            var path = string.IsNullOrEmpty(parentPath) ? location : $"{parentPath}{location}";

            if (!results.IsValid && !string.IsNullOrWhiteSpace(results.Message))
            {
                yield return new ValidationError
                {
                    PropertyName = path,
                    ErrorMessage = results.Message,
                    Source = "JsonSchema.Net"
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
