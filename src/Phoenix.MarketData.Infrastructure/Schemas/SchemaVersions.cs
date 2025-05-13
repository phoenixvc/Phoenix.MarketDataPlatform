namespace Phoenix.MarketData.Infrastructure.Schemas;

/// <summary>
/// Provides schema versioning information and supported schema versions
/// for the Phoenix MarketData domain.
/// </summary>
public static class SchemaVersions
{
    /// <summary>
    /// Initial schema version (SemVer: Major.Minor.Patch)
    /// </summary>
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
        if (string.IsNullOrEmpty(version1) || string.IsNullOrEmpty(version2))
            return false;

        try
        {
            var v1 = Version.Parse(version1);
            var v2 = Version.Parse(version2);
        
            return v1 > v2;
        }
        catch (FormatException)
        {
            throw new FormatException("Could not parse version string when trying to compare versions.");
        }
    }

    /// <summary>
    /// Returns the latest supported schema version.
    /// </summary>
    /// <returns>The latest supported schema version.</returns>
    public static string GetLatestVersion()
    {
        try
        {
            return Supported.Count > 0 
                ? Supported.OrderBy(Version.Parse).Last() 
                : string.Empty;
        }
        catch (FormatException ex)
        {
            // Log the error and return a fallback version or rethrow with better context
            throw new FormatException("Invalid version format found in supported versions.", ex);
        }
    }
}