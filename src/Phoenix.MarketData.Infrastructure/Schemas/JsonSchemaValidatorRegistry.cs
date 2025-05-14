namespace Phoenix.MarketData.Infrastructure.Schemas;

/// <summary>
/// Provides a centralized registry for managing and retrieving JSON schema validators.
/// This class is responsible for maintaining a collection of schema file paths associated
/// with specific schema keys that identify the data type, asset class, and schema version.
/// </summary>
public class JsonSchemaValidatorRegistry
{
    public static JsonSchemaValidatorRegistry Validator = new ();

    /// <summary>
    /// Represents a collection of validator paths stored as key-value pairs,
    /// where the key is a string representing the schema name or identifier (),
    /// and the value is a string representing the file path or location of the schema.
    /// </summary>
    private Dictionary<SchemaKey, string> _validatorPaths = new Dictionary<SchemaKey, string>();
    
    protected JsonSchemaValidatorRegistry()
    {
        _validatorPaths.Add(
            new SchemaKey("price.spot", "fx", "1.0.0"),
            Path.Combine(AppContext.BaseDirectory, "Schemas", "FxSpotPriceData_v1.0.0.schema.json"));
        _validatorPaths.Add(
            new SchemaKey("price.ordinals.spot", "crypto", "1.0.0"),
            Path.Combine(AppContext.BaseDirectory, "Schemas", "CryptoOrdinalSpotPriceData_v1.0.0.schema.json"));
    }

    public bool Validate(string dataType, string assetClass, string schemaVersion, string json, out string errorMsg)
    {
        var schemaKey = new SchemaKey(dataType, assetClass, schemaVersion);
        if (!_validatorPaths.TryGetValue(schemaKey, out var path))
        {
            errorMsg = "Could not find validator for schema key: " + schemaKey;
            return false;
        }
        
        var validator = new JsonSchemaValidator(path);
        return validator.Validate(json, out errorMsg);
    }
}

public record struct SchemaKey
{
    public SchemaKey(string dataType, string assetClass, string schemaVersion)
    {
        DataType = dataType;
        AssetClass = assetClass;
        SchemaVersion = schemaVersion;
    }

    public string AssetClass { get; }
    public string DataType { get; }
    public string SchemaVersion { get; }
}