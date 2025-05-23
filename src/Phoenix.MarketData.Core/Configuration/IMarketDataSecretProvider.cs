using System.Threading;
using System.Threading.Tasks;

namespace Phoenix.MarketData.Domain.Configuration
{
    /// <summary>
    /// Provides secure access to application secrets
    /// </summary>
    public interface IMarketDataSecretProvider
    {
        /// <summary>
        /// Gets the Cosmos DB connection string
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
        /// <returns>The Cosmos DB connection string</returns>
        Task<string> GetCosmosConnectionStringAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the Event Grid API key
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
        /// <returns>The Event Grid API key</returns>
        Task<string> GetEventGridKeyAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the Event Grid endpoint
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
        /// <returns>The Event Grid endpoint URL</returns>
        Task<string> GetEventGridEndpointAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the Event Hub connection string
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation</param>
        /// <returns>The Event Hub connection string</returns>
        Task<string> GetEventHubConnectionStringAsync(CancellationToken cancellationToken = default);
    }
}