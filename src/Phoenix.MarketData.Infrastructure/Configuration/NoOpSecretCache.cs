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
    }
}