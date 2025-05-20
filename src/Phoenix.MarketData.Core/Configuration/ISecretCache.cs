namespace Phoenix.MarketData.Core.Configuration
{
    /// <summary>
    /// Interface for caching secrets to reduce calls to external secret stores
    /// </summary>
    public interface ISecretCache
    {
        /// <summary>
        /// Retrieves a secret from the cache
        /// </summary>
        /// <param name="secretName">Name of the secret to retrieve</param>
        /// <returns>The secret value if found, null otherwise</returns>
        string? GetSecret(string secretName);

        /// Caches a secret for future retrieval
        /// </summary>
        /// <param name="secretName">Name of the secret to cache</param>
        /// <param name="value">Secret value to cache</param>
        void CacheSecret(string secretName, string value);
    }
}