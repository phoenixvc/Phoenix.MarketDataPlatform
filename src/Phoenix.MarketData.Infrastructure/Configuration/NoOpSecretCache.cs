using System;
using Phoenix.MarketData.Core.Configuration;

namespace Phoenix.MarketData.Infrastructure.Configuration
{
    /// <summary>
    /// No-operation implementation of ISecretCache for when caching isn't required
    /// </summary>
    internal class NoOpSecretCache : ISecretCache
    {
        public string? GetSecret(string secretName) => null;

        public void CacheSecret(string secretName, string value) { }

        /// <summary>
        /// Gets a secret with its expiration information - no-op implementation returns null
        /// </summary>
        public (string? Value, bool IsExpired) GetSecretWithExpiration(string secretName) => (null, false);

        /// <summary>
        /// Tries to get a secret with expiration information - no-op implementation always returns false
        /// </summary>
        public bool TryGetSecret(string secretName, out string? value, out bool isExpired)
        {
            value = null;
            isExpired = false;
            return false;
        }

        /// <summary>
        /// Caches a secret with expiration information - no-op implementation does nothing
        /// </summary>
        public void CacheSecretWithExpiration(string secretName, string value, DateTimeOffset expiration) { }
    }
}
