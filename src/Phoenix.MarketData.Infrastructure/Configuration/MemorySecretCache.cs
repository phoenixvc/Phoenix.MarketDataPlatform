using System;
using System.Collections.Concurrent;
using Phoenix.MarketData.Domain.Configuration;

namespace Phoenix.MarketData.Infrastructure.Configuration
{
    public class MemorySecretCache : ISecretCache
    {
        private readonly ConcurrentDictionary<string, CachedSecret> _cache = new();

        public string? GetSecret(string secretName)
        {
            if (_cache.TryGetValue(secretName, out var cachedSecret))
            {
                return cachedSecret.Value;
            }
            return null;
        }

        // Keep old method for backward compatibility (can be removed if not needed)
        public (string? Value, bool IsExpired) GetSecretWithExpiration(string secretName)
        {
            if (_cache.TryGetValue(secretName, out var cachedSecret))
            {
                bool isExpired = cachedSecret.ExpiresOn <= DateTimeOffset.UtcNow;
                return (cachedSecret.Value, isExpired);
            }
            return (null, true);
        }

        // Add new method required by interface
        public bool TryGetSecret(string secretName, out string? value, out bool isExpired)
        {
            if (_cache.TryGetValue(secretName, out var cachedSecret))
            {
                value = cachedSecret.Value;
                isExpired = cachedSecret.ExpiresOn <= DateTimeOffset.UtcNow;
                return true;
            }

            value = null;
            isExpired = true;
            return false;
        }

        public void CacheSecret(string secretName, string value)
        {
            // Default to 1 hour expiration if not specified
            CacheSecretWithExpiration(secretName, value, DateTimeOffset.UtcNow.AddHours(1));
        }

        public void CacheSecretWithExpiration(string secretName, string value, DateTimeOffset expiresOn)
        {
            var cachedSecret = new CachedSecret(value, expiresOn);
            _cache.AddOrUpdate(secretName, cachedSecret, (_, _) => cachedSecret);
        }

        private class CachedSecret
        {
            public CachedSecret(string value, DateTimeOffset expiresOn)
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("Secret value cannot be null or empty", nameof(value));
                if (expiresOn <= DateTimeOffset.UtcNow)
                    throw new ArgumentException("Expiration must be a future date/time", nameof(expiresOn));
                Value = value;
                ExpiresOn = expiresOn;
            }

            public string Value { get; }
            public DateTimeOffset ExpiresOn { get; }
        }
    }
}