namespace Phoenix.MarketData.Core;

/// <summary>
/// Provides predefined constants representing various regions for market data classification.
/// </summary>
public static class Regions
{
    /// <summary>
    /// Represents global market data that is not specific to any regional market.
    /// </summary>
    public const string Global = "global";

    /// <summary>
    /// Represents the New York market region (NY).
    /// Used for North American market data including US equities, options, and related instruments.
    /// </summary>
    public const string NewYork = "ny";

    /// <summary>
    /// Represents the London market region (LDN).
    /// Used for European market data including UK and EU equities, options, and related instruments.
    /// </summary>
    public const string London = "ldn";

    /// <summary>
    /// Represents the Johannesburg market region (JHB).
    /// Used for South African market data including JSE-listed equities and related instruments.
    /// </summary>
    public const string Johannesburg = "jhb";
}