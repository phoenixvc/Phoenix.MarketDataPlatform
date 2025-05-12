namespace Phoenix.MarketData.Domain.Schemas;

/// <summary>
/// Provides schema versioning information and supported schema versions
/// for the Phoenix MarketData domain.
/// </summary>
public static class SchemaVersions
{
    public const string V0 = "0.0.0";
    
    public static readonly HashSet<string> Supported = [V0];
}