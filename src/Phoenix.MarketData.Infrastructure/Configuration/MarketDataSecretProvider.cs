using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Core.Configuration;
using Azure;

namespace Phoenix.MarketData.Infrastructure.Configuration
{
    public class MarketDataSecretProvider : IMarketDataSecretProvider
    {
        private readonly SecretClient _secretClient;
        private readonly ISecretCache _secretCache;
        private readonly ILogger<MarketDataSecretProvider>? _logger;
        private readonly TimeSpan _defaultSecretLifetime = TimeSpan.FromHours(4); // Default expiration of 4 hours

        public MarketDataSecretProvider(
            string keyVaultUrl,
            ISecretCache? secretCache = null,
            ILogger<MarketDataSecretProvider>? logger = null)
        {
            if (string.IsNullOrEmpty(keyVaultUrl))
                throw new ArgumentException("Key Vault URL cannot be null or empty", nameof(keyVaultUrl));

            // Configure client options with proper retry settings
            var clientOptions = new SecretClientOptions()
            {
                // Configure retry settings directly on the options
                Retry = {
                    Delay = TimeSpan.FromSeconds(1),
                    MaxDelay = TimeSpan.FromSeconds(10),
                    MaxRetries = 3,
                    Mode = RetryMode.Exponential
                }
            };

            // Use DefaultAzureCredential for managed identity or local development
            _secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential(), clientOptions);
            _secretCache = secretCache ?? new NoOpSecretCache();
            _logger = logger;
        }

        // Original methods without CancellationToken - delegate to the new methods with default CancellationToken
        public Task<string> GetCosmosConnectionStringAsync()
        {
            return GetCosmosConnectionStringAsync(CancellationToken.None);
        }

        public Task<string> GetEventGridKeyAsync()
        {
            return GetEventGridKeyAsync(CancellationToken.None);
        }

        public Task<string> GetEventGridEndpointAsync()
        {
            return GetEventGridEndpointAsync(CancellationToken.None);
        }

        public Task<string> GetEventHubConnectionStringAsync()
        {
            return GetEventHubConnectionStringAsync(CancellationToken.None);
        }

        // New methods with CancellationToken parameter to match the interface
        public Task<string> GetCosmosConnectionStringAsync(CancellationToken cancellationToken)
        {
            return GetSecretAsync("CosmosDbConnectionString", cancellationToken);
        }

        public Task<string> GetEventGridKeyAsync(CancellationToken cancellationToken)
        {
            return GetSecretAsync("EventGridKey", cancellationToken);
        }

        public Task<string> GetEventGridEndpointAsync(CancellationToken cancellationToken)
        {
            return GetSecretAsync("EventGridEndpoint", cancellationToken);
        }

        public Task<string> GetEventHubConnectionStringAsync(CancellationToken cancellationToken)
        {
            return GetSecretAsync("EventHubConnectionString", cancellationToken);
        }

        // Updated method with cancellation token
        private async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));

            try
            {
                // Try to get from cache first
                string? cachedValue;
                bool isCacheExpired;

                // Use the new TryGetSecret method instead of GetSecretWithExpiration
                if (_secretCache.TryGetSecret(secretName, out cachedValue, out isCacheExpired))
                {
                    if (!string.IsNullOrEmpty(cachedValue) && !isCacheExpired)
                    {
                        _logger?.LogDebug("Retrieved secret '{SecretName}' from cache", secretName);
                        return cachedValue;
                    }
                }
                else
                {
                    // Fallback to GetSecret if TryGetSecret returns false
                    cachedValue = _secretCache.GetSecret(secretName);
                    // Assume it's expired if we need to fall back to GetSecret
                    isCacheExpired = true;
                }

                // If cache is expired or empty, retrieve from Key Vault
                _logger?.LogDebug("Fetching secret '{SecretName}' from Key Vault", secretName);

                // Fixed: Use named parameters to ensure the correct overload is called
                var response = await _secretClient.GetSecretAsync(
                    name: secretName,
                    version: null, // Get latest version
                    cancellationToken: cancellationToken);
                var secret = response?.Value;

                if (secret == null)
                {
                    _logger?.LogWarning("Retrieved null secret for '{SecretName}' from Key Vault", secretName);

                    // If we have an expired cached value, we can use it as a fallback
                    if (!string.IsNullOrEmpty(cachedValue))
                    {
                        _logger?.LogWarning("Using expired cached value for '{SecretName}' as fallback", secretName);
                        return cachedValue;
                    }

                    throw new InvalidOperationException($"Secret '{secretName}' not found in Key Vault");
                }

                var value = secret.Value;

                // Get expiration date from Key Vault if available, or use default expiration
                DateTimeOffset expiresOn = secret.Properties.ExpiresOn ?? DateTimeOffset.UtcNow.Add(_defaultSecretLifetime);

                // Cache the secret with expiration
                _secretCache.CacheSecretWithExpiration(secretName, value, expiresOn);

                _logger?.LogDebug("Successfully retrieved and cached secret '{SecretName}'", secretName);
                return value;
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                _logger?.LogError(ex, "Secret '{SecretName}' not found in Key Vault", secretName);
                throw new KeyNotFoundException($"Secret '{secretName}' not found in Key Vault", ex);
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.Forbidden ||
                                                   ex.Status == (int)HttpStatusCode.Unauthorized)
            {
                _logger?.LogError(ex, "Access denied to secret '{SecretName}' in Key Vault", secretName);
                throw new AccessDeniedException($"Access denied to secret '{secretName}' in Key Vault", ex);
            }
            catch (RequestFailedException ex) when (IsTransientError(ex.Status))
            {
                _logger?.LogWarning(ex, "Transient error accessing Key Vault for secret '{SecretName}'", secretName);

                // For transient errors, try to use cached value even if expired
                var cachedValue = _secretCache.GetSecret(secretName);
                if (!string.IsNullOrEmpty(cachedValue))
                {
                    _logger?.LogInformation("Using cached value for '{SecretName}' due to Key Vault transient error", secretName);
                    return cachedValue;
                }

                _logger?.LogError("No cached value available for '{SecretName}' when Key Vault is unavailable", secretName);
                throw new ServiceUnavailableException("Key Vault service is unavailable", ex);
            }
            catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                _logger?.LogWarning(ex, "Operation to retrieve secret '{SecretName}' was canceled", secretName);
                throw; // Rethrow cancellation exceptions
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error retrieving secret '{SecretName}' from Key Vault", secretName);
                throw new SecretProviderException($"Error retrieving secret '{secretName}' from Key Vault", ex);
            }
        }

        private bool IsTransientError(int statusCode)
        {
            // Consider these status codes as transient failures
            return statusCode == (int)HttpStatusCode.RequestTimeout ||
                   statusCode == (int)HttpStatusCode.ServiceUnavailable ||
                   statusCode == (int)HttpStatusCode.GatewayTimeout ||
                   statusCode == (int)HttpStatusCode.TooManyRequests;
        }
    }

    // Custom exception classes for better error handling
    public class KeyNotFoundException : Exception
    {
        public KeyNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class SecretProviderException : Exception
    {
        public SecretProviderException(string message, Exception innerException) : base(message, innerException) { }
    }
}
