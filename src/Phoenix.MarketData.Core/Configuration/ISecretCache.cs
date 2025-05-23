using System;

namespace Phoenix.MarketData.Domain.Configuration
{
    public interface ISecretCache
    {
        /// <summary>
        /// Gets a secret from the cache by name
        /// </summary>
        /// <param name="secretName">The name of the secret to retrieve</param>
        /// <returns>The secret value, or null if not found</returns>
        string? GetSecret(string secretName);

        /// <summary>
        /// Gets a secret and its expiration status from the cache
        /// </summary>
        /// <param name="secretName">The name of the secret to retrieve</param>
        /// <returns>True if the secret was found in the cache, false otherwise</returns>
        bool TryGetSecret(
            string secretName,
            out string? value,
            out bool isExpired);

        /// <summary>
        /// Caches a secret with the specified name and value
        /// </summary>
        /// <param name="secretName">The name of the secret</param>
        /// <param name="value">The secret value</param>
        void CacheSecret(string secretName, string value);

        /// <summary>
        /// Caches a secret with the specified name, value, and expiration time
        /// </summary>
        /// <param name="secretName">The name of the secret</param>
        /// <param name="value">The secret value</param>
        /// <param name="expiresOn">When the cached secret expires</param>
        void CacheSecretWithExpiration(string secretName, string value, DateTimeOffset expiresOn);
    }
}