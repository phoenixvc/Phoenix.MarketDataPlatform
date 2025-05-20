using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Phoenix.MarketData.Infrastructure.Configuration
{
    public class MarketDataSecretProvider : IMarketDataSecretProvider
    {
        private readonly SecretClient _secretClient;

        public MarketDataSecretProvider(string keyVaultUrl)
        {
            if (string.IsNullOrEmpty(keyVaultUrl))
                throw new ArgumentException("Key Vault URL cannot be null or empty", nameof(keyVaultUrl));

            // Use DefaultAzureCredential for managed identity or local development
            _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        }

        public async Task<string> GetCosmosConnectionStringAsync()
        {
            var secret = await _secretClient.GetSecretAsync("CosmosDbConnectionString");
            return secret.Value.Value;
        }

        public async Task<string> GetEventGridKeyAsync()
        {
            var secret = await _secretClient.GetSecretAsync("EventGridKey");
            return secret.Value.Value;
        }
    }
}