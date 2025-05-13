using System.Net;
using Microsoft.Azure.Cosmos;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Domain.Models.Interfaces;
using Phoenix.MarketData.Infrastructure.Mapping;

namespace Phoenix.MarketData.Infrastructure.Cosmos
{
    public class MarketDataRepository
    {
        private readonly Container _container;
        private VersionManager _versionManager;

        public MarketDataRepository(CosmosClient cosmosClient, string databaseId, string containerId)
        {
            _container = cosmosClient.GetContainer(databaseId, containerId);
            _versionManager = new VersionManager(this);
        }

        /// <summary>
        /// Saves a market data object asynchronously to the configured Cosmos DB container.
        /// </summary>
        /// <typeparam name="T">The type of the market data object, implementing IMarketDataObject interface.</typeparam>
        /// <param name="marketData">The market data object to be saved.</param>
        /// <param name="saveNextVersion">Boolean indicating whether to save to the next version (latest) of the
        /// market data object</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task SaveAsync<T>(T marketData, bool saveNextVersion = true) where T : IMarketDataObject
        {
            const int maxRetries = 3;

            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                if (saveNextVersion)
                {
                    marketData.Version = await _versionManager.GetNextVersionAsync<T>(
                        marketData.DataType,
                        marketData.AssetClass,
                        marketData.AssetId,
                        marketData.Region,
                        marketData.AsOfDate,
                        marketData.DocumentType);
                }

                var dto = MapToDto(marketData);

                try
                {
                    await _container.CreateItemAsync(dto, new PartitionKey(marketData.AssetId));
                    return;
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict && saveNextVersion)
                {
                    // Another writer inserted the same version before us, try again
                    if (attempt == maxRetries)
                        throw;

                    await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt)); // simple backoff
                }
            }

            throw new Exception("Failed to save market data after multiple retries due to version conflict.");
        }


        public async Task<T?> GetLatestAsync<T>(string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType) where T : IMarketDataObject
        {
            var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.assetId = @assetId AND c.assetClass = @assetClass AND c.region = @region AND c.dataType = @dataType AND c.documentType = @documentType AND c.asOfDate = @asOfDate ORDER BY c.version DESC")
                .WithParameter("@assetId", assetId)
                .WithParameter("@assetClass", assetClass)
                .WithParameter("@region", region)
                .WithParameter("@dataType", dataType)
                .WithParameter("@documentType", documentType)
                .WithParameter("@asOfDate", asOfDate);

            using var feedIterator = _container.GetItemQueryIterator<T>(query, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(assetId),
                MaxBufferedItemCount = 1,
                MaxItemCount = 1
            });

            if (!feedIterator.HasMoreResults) return default;
            var response = await feedIterator.ReadNextAsync();
            return response.FirstOrDefault();
        }
        
        private object MapToDto<T>(T domain) where T : IMarketDataObject
        {
            return domain switch
            {
                FxSpotPriceData fxSpotData => FxSpotPriceDataMapper.ToDto(fxSpotData),
                //Add other mappers...
                //ForwardPriceData f => ForwardPriceMapper.ToDto(f),
                //YieldCurveData y => YieldCurveMapper.ToDto(y),
                //VolatilitySurfaceData v => VolatilitySurfaceMapper.ToDto(v),
                _ => throw new NotSupportedException($"Unsupported market data type: {typeof(T).Name}")
            };
        }
    }
}