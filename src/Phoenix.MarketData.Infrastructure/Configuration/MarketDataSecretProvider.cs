using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Phoenix.MarketData.Core.Configuration;

namespace Phoenix.MarketData.Infrastructure.Configuration
{
    public class MarketDataSecretProvider : IMarketDataSecretProvider
    {
        private readonly SecretClient _secretClient;
        private readonly ISecretCache _secretCache;

        public MarketDataSecretProvider(string keyVaultUrl, ISecretCache? secretCache = null)
        {
            if (string.IsNullOrEmpty(keyVaultUrl))
                throw new ArgumentException("Key Vault URL cannot be null or empty", nameof(keyVaultUrl));

            // Use DefaultAzureCredential for managed identity or local development
            _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            _secretCache = secretCache ?? new NoOpSecretCache();
        }

        public async Task<string> GetCosmosConnectionStringAsync()
        {
            return await GetSecretAsync("CosmosDbConnectionString");
        }

        public async Task<string> GetEventGridKeyAsync()
        {
            return await GetSecretAsync("EventGridKey");
        }
        
        public async Task<string> GetEventGridEndpointAsync()
        {
            return await GetSecretAsync("EventGridEndpoint");
        }
        
        public async Task<string> GetEventHubConnectionStringAsync()
        {
            return await GetSecretAsync("EventHubConnectionString");
        }
        
        private async Task<string> GetSecretAsync(string secretName)
        {
            // Try to get from cache first
            var cachedValue = _secretCache.GetSecret(secretName);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return cachedValue;
            }
            
            // Retrieve from Key Vault
            var secret = await _secretClient.GetSecretAsync(secretName);
            var value = secret.Value.Value;
            
            // Cache the secret
            _secretCache.CacheSecret(secretName, value);
            
            return value;
        }
    }
}