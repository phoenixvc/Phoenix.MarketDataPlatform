using System.Threading.Tasks;

namespace Phoenix.MarketData.Core.Configuration
{
    /// <summary>
    /// Provides secure access to application secrets
    /// </summary>
    public interface IMarketDataSecretProvider
    {
        /// <summary>
        /// Gets the Cosmos DB connection string
        /// </summary>
        Task<string> GetCosmosConnectionStringAsync();
        
        /// <summary>
        /// Gets the Event Grid API key
        /// </summary>
        Task<string> GetEventGridKeyAsync();
        
        /// <summary>
        /// Gets the Event Grid endpoint
        /// </summary>
        Task<string> GetEventGridEndpointAsync();
        
        /// <summary>
        /// Gets the Event Hub connection string
        /// </summary>
        Task<string> GetEventHubConnectionStringAsync();
    }
}