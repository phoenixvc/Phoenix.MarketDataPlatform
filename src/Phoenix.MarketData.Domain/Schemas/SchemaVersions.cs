namespace Phoenix.MarketData.Domain.Schemas;

/// <summary>
/// Provides schema versioning information and supported schema versions
/// for the Phoenix MarketData domain.
/// </summary>
public static class SchemaVersions
{
    public const string V0 = "0.0.0";
    // Ready for future versions
    // public const string V1 = "1.0.0";

    /// <summary>
    /// Represents the collection of schema versions supported within the Phoenix MarketData domain.
    /// This set of versions is used to validate and check compatibility for schema versioning in
    /// operations and data handling.
    /// </summary>
    public static readonly HashSet<string> Supported = [V0];
    
    /// <summary>
    /// Checks if a schema version is supported
    /// </summary>
    public static bool IsSupported(string version) => Supported.Contains(version);
    
    /// <summary>
    /// Compares two versions and returns true if version1 is newer than version2
    /// </summary>
    public static bool IsNewerThan(string version1, string version2)
    {
        // Implementation for semantic version comparison
        return false; // Placeholder
    }
}