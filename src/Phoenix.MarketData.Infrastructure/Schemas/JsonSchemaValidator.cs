using System.Text.Json;
using Json.Schema;

namespace Phoenix.MarketData.Infrastructure.Schemas;

public class JsonSchemaValidator
{
    private readonly JsonSchema _schema;

    public JsonSchemaValidator(string schemaFilePath)
    {
        ArgumentNullException.ThrowIfNull(schemaFilePath);
        try
        {
            var schemaJson = File.ReadAllText(schemaFilePath);
            _schema = JsonSchema.FromText(schemaJson);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to read schema file: {schemaFilePath}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse schema JSON from file: {schemaFilePath}", ex);
        }
    }

    public ValidationResult Validate(string jsonPayload)
    {
        var element = JsonDocument.Parse(jsonPayload).RootElement;
        var result = _schema.Evaluate(element, new EvaluationOptions
        {
            OutputFormat = OutputFormat.Hierarchical,
            ValidateAgainstMetaSchema = true
        });

        if (result.IsValid)
            return ValidationResult.Success();

        // Failed to validate
        var errorMessage = string.Join("; ", result.Details.Select(
            e => e.InstanceLocation + ": " + (e.HasErrors
                ? string.Join("\n", e.Errors?.Select(kvp => kvp.Key.ToString() + " - " + kvp.Value.ToString()) ?? Array.Empty<string>())
                : "")));
        return ValidationResult.Failure(errorMessage);
    }
}

public class ValidationResult
{
    public bool IsValid { get; }
    public string ErrorMessage { get; }

    private ValidationResult(bool isValid, string errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new ValidationResult(true);
    public static ValidationResult Failure(string errorMessage) => new ValidationResult(false, errorMessage);
}