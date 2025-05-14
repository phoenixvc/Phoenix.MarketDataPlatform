using System.Text.Json;
using Json.Schema;

namespace Phoenix.MarketData.Infrastructure.Schemas;

public class JsonSchemaValidator
{
    private readonly JsonSchema _schema;

    public JsonSchemaValidator(string schemaFilePath)
    {
        var schemaJson = File.ReadAllText(schemaFilePath);
        _schema = JsonSchema.FromText(schemaJson);
    }

    public bool Validate(string jsonPayload, out string errorMessage)
    {
        var element = JsonDocument.Parse(jsonPayload).RootElement;
        var result = _schema.Evaluate(element, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

        if (result.IsValid)
        {
            errorMessage = null;
            return true;
        }

        // Failed to validate
        errorMessage = string.Join("; ", result.Details.Select(
            e => e.InstanceLocation + ": " + (e.HasErrors
                ? string.Join("\n", e.Errors?.Select(kvp => kvp.Key.ToString() + " - " + kvp.Value.ToString()) ?? Array.Empty<string>())
                : "")));
        return false;
    }
}