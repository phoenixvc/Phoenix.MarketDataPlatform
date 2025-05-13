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
        private readonly VersionManager _versionManager;

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
        public async Task<SaveMarketDataResult> SaveAsync<T>(T marketData, bool saveNextVersion = true) where T : IMarketData
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
                    return new SaveMarketDataResult { Success = true, Id = marketData.Id, Version = marketData.Version };
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
                {
                    // Handle case where access is denied to the resource
                    return new SaveMarketDataResult
                    {
                        Success = false,
                        Exception = ex,
                        Message = "Access to the Cosmos DB container is forbidden. Check permissions."
                    };
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                {
                    // Handle case where the item size exceeds the maximum allowed limit
                    return new SaveMarketDataResult
                    {
                        Success = false,
                        Exception = ex,
                        Message = "The market data item exceeds the maximum allowed size in Cosmos DB."
                    };
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    // Handle case where the Cosmos DB service is temporarily unavailable
                    return new SaveMarketDataResult
                    {
                        Success = false,
                        Exception = ex,
                        Message = "Cosmos DB service is temporarily unavailable. Please try again later."
                    };
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    // Handle case where the request rate exceeds the allotted RU/s
                    return new SaveMarketDataResult
                    {
                        Success = false,
                        Exception = ex,
                        Message = "The request rate is too high. Consider retrying after some time or increasing RU/s."
                    };
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict && saveNextVersion)
                {
                    // Another writer inserted the same version before us, try again
                    if (attempt == maxRetries)
                        throw;

                    await Task.Delay(TimeSpan.FromMilliseconds(50 * attempt)); // simple backoff
                }
                catch (Exception ex)
                {
                    return new SaveMarketDataResult { Success = false, Exception = ex, Message = "Failed to save market data." };
                }
            }

            return new SaveMarketDataResult { Success = false, Message = "Failed to save market data after multiple retries due to version conflict." };
        }
        
        public async Task<LoadMarketDataResult<T>> GetLatestAsync<T>(string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType) where T : IMarketData
        {
            var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.assetId = @assetId AND c.assetClass = @assetClass AND c.region = @region AND c.dataType = @dataType AND c.documentType = @documentType AND c.asOfDate = @asOfDate ORDER BY c.version DESC")
                .WithParameter("@assetId", assetId)
                .WithParameter("@assetClass", assetClass)
                .WithParameter("@region", region)
                .WithParameter("@dataType", dataType)
                .WithParameter("@documentType", documentType)
                .WithParameter("@asOfDate", asOfDate);

            return await ExecuteQuery<T>(assetId, query);
        }
        
        public async Task<LoadMarketDataResult<T>> GetAsync<T>(string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType, int version) where T : IMarketData
        {
            var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.assetId = @assetId AND c.assetClass = @assetClass AND c.region = @region AND c.dataType = @dataType AND c.documentType = @documentType AND c.asOfDate = @asOfDate AND c.version = @version")
                .WithParameter("@assetId", assetId)
                .WithParameter("@assetClass", assetClass)
                .WithParameter("@region", region)
                .WithParameter("@dataType", dataType)
                .WithParameter("@documentType", documentType)
                .WithParameter("@asOfDate", asOfDate)
                .WithParameter("@version", version);

            return await ExecuteQuery<T>(assetId, query);
        }

        private async Task<LoadMarketDataResult<T>> ExecuteQuery<T>(string assetId, QueryDefinition query) where T : IMarketData
        {
            try
            {
                using var feedIterator = _container.GetItemQueryIterator<T>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(assetId),
                    MaxBufferedItemCount = 1,
                    MaxItemCount = 1
                });

                if (!feedIterator.HasMoreResults) return new LoadMarketDataResult<T>{ Success = false, Message = "Market data not found."};
                var response = await feedIterator.ReadNextAsync();
                return new LoadMarketDataResult<T> { Success = true, Result = response.FirstOrDefault() };
            }
            catch (CosmosException cosmosEx)
            {
                return new LoadMarketDataResult<T>
                {
                    Success = false,
                    Exception = cosmosEx,
                    Message = $"CosmosException occurred with status code {cosmosEx.StatusCode}: {cosmosEx.Message}"
                };
            }
            catch (InvalidOperationException invalidOpEx)
            {
                return new LoadMarketDataResult<T>
                {
                    Success = false,
                    Exception = invalidOpEx,
                    Message = "An invalid operation occurred while loading market data."
                };
            }
            catch (TimeoutException timeoutEx)
            {
                return new LoadMarketDataResult<T>
                {
                    Success = false,
                    Exception = timeoutEx,
                    Message = "A timeout occurred while attempting to load market data."
                };
            }
            catch (Exception e)
            {
                return new LoadMarketDataResult<T>
                {
                    Success = false,
                    Exception = e,
                    Message = "An unexpected exception occurred while loading market data."
                };
            }
        }

        private object MapToDto<T>(T domain) where T : IMarketData
        {
            return domain switch
            {
                FxSpotPriceData fxSpotData => FxSpotPriceDataMapper.ToDto(fxSpotData),
                CryptoOrdinalSpotPriceData cryptoSpotData => CryptoOrdinalSpotPriceDataMapper.ToDto(cryptoSpotData),
                //Add other mappers...
                //ForwardPriceData f => ForwardPriceMapper.ToDto(f),
                //YieldCurveData y => YieldCurveMapper.ToDto(y),
                //VolatilitySurfaceData v => VolatilitySurfaceMapper.ToDto(v),
                _ => throw new NotSupportedException($"Unsupported market data type: {typeof(T).Name}")
            };
        }
    }

    public class SaveMarketDataResult
    {
        public required bool Success { get; set; }

        public string? Id { get; set; } = string.Empty;
        
        public int? Version { get; set; } = null;

        public Exception? Exception { get; set; } = null;
        
        public string? Message { get; set; } = string.Empty;
    }

    public class LoadMarketDataResult<T> where T : IMarketData
    {
        public required bool Success { get; set; }

        public T? Result { get; set; } = default;
        
        public Exception? Exception { get; set; } = null;
        
        public string? Message { get; set; } = string.Empty;
    }
}