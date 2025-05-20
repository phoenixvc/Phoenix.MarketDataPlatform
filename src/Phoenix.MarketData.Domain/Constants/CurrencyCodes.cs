namespace Phoenix.MarketData.Domain.Constants
{
    /// <summary>
    /// Standardized currency code constants (ISO 4217).
    /// </summary>
    public static class CurrencyCodes
    {
        public const string USD = "USD";
        public const string EUR = "EUR";
        public const string GBP = "GBP";
        public const string ZAR = "ZAR";
        public const string JPY = "JPY";
        public const string CHF = "CHF";
        public const string AUD = "AUD";
        public const string CAD = "CAD";
        public const string CNY = "CNY";
        public const string BTC = "BTC";
        public const string ETH = "ETH";
        // Add more as you need, but don’t add tokens or meme coins unless you’re running a casino.

        /// <summary>
        /// Returns a list of all defined codes.
        /// </summary>
        public static IReadOnlyList<string> All => new[]
        {
            USD, EUR, GBP, ZAR, JPY, CHF, AUD, CAD, CNY, BTC, ETH
        };
    }
}
