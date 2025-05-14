using System.Net;
using Microsoft.Azure.Cosmos;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Domain.Models.Interfaces;
using Phoenix.MarketData.Infrastructure.Cosmos;
using Phoenix.MarketData.Infrastructure.Mapping;
using Phoenix.MarketData.Infrastructure.Schemas;

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

        public MarketDataRepository(Container container)
        {
            _container = container;
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
        public async Task<SaveMarketDataResult> SaveMarketDataAsync<T>(T marketData, bool saveNextVersion = true) where T : IMarketData
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

        /// <summary>
        /// Retrieves the latest version of a market data object asynchronously based on the specified criteria.
        /// </summary>
        /// <typeparam name="T">The type of the market data object, implementing the IMarketData interface.</typeparam>
        /// <param name="dataType">The type of the market data (e.g., pricing, analytics, etc.).</param>
        /// <param name="assetClass">The class of the asset (e.g., equity, fixed income, etc.).</param>
        /// <param name="assetId">The unique identifier for the asset.</param>
        /// <param name="region">The region associated with the market data.</param>
        /// <param name="asOfDate">The "as of" date representing the date to filter the market data.</param>
        /// <param name="documentType">The type of document associated with the market data.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the query with
        /// the latest version of the market data object.</returns>
        public async Task<LoadMarketDataResult<T>> GetMarketDataByLatestVersionAsync<T>(string dataType, string assetClass,
            string assetId, string region,
            DateOnly asOfDate, string documentType) where T : IMarketData
        {
            var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.assetId = @assetId AND c.assetClass = @assetClass AND c.region = @region AND c.dataType = @dataType AND c.documentType = @documentType AND c.asOfDate = @asOfDate ORDER BY c.version DESC")
                .WithParameter("@assetId", assetId.ToLowerInvariant())
                .WithParameter("@assetClass", assetClass.ToLowerInvariant())
                .WithParameter("@region", region.ToLowerInvariant())
                .WithParameter("@dataType", dataType.ToLowerInvariant())
                .WithParameter("@documentType", documentType.ToLowerInvariant())
                .WithParameter("@asOfDate", asOfDate);

            return await ExecuteMarketDataFetchQuery<T>(assetId, query);
        }

        /// <summary>
        /// Retrieves a specific version of the market data object asynchronously from the configured Cosmos DB container.
        /// </summary>
        /// <typeparam name="T">The type of the market data object, implementing the IMarketData interface.</typeparam>
        /// <param name="dataType">The type of the market data (e.g., price, reference data).</param>
        /// <param name="assetClass">The class of the asset (e.g., equity, fixed income).</param>
        /// <param name="assetId">The unique identifier of the asset.</param>
        /// <param name="region">The region associated with the market data.</param>
        /// <param name="asOfDate">The "as of" date for which the market data is valid.</param>
        /// <param name="documentType">The type of the document (e.g., metadata, analytics).</param>
        /// <param name="version">The specific version number of the market data to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation, containing the result of the market data retrieval.</returns>
        public async Task<LoadMarketDataResult<T>> GetMarketDataBySpecifiedVersionAsync<T>(string dataType, string assetClass,
            string assetId, string region,
            DateOnly asOfDate, string documentType, int version) where T : IMarketData
        {
            var query = new QueryDefinition(
                    "SELECT TOP 1 * FROM c WHERE c.assetId = @assetId AND c.assetClass = @assetClass AND c.region = @region AND c.dataType = @dataType AND c.documentType = @documentType AND c.asOfDate = @asOfDate AND c.version = @version")
                .WithParameter("@assetId", assetId.ToLowerInvariant())
                .WithParameter("@assetClass", assetClass.ToLowerInvariant())
                .WithParameter("@region", region.ToLowerInvariant())
                .WithParameter("@dataType", dataType.ToLowerInvariant())
                .WithParameter("@documentType", documentType.ToLowerInvariant())
                .WithParameter("@asOfDate", asOfDate)
                .WithParameter("@version", version);

            return await ExecuteMarketDataFetchQuery<T>(assetId, query);
        }

        /// <summary>
        /// Executes a query to fetch market data from the Cosmos DB container.
        /// </summary>
        /// <typeparam name="T">The type of the market data object, implementing IMarketData interface.</typeparam>
        /// <param name="assetId">The unique identifier for the asset partition to retrieve data from.</param>
        /// <param name="query">The Cosmos DB query definition specifying the criteria to fetch market data.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a LoadMarketDataResult object indicating the success of the operation and the fetched market data.</returns>
        public async Task<LoadMarketDataResult<T>> ExecuteMarketDataFetchQuery<T>(string assetId, QueryDefinition query)
            where T : IMarketData
        {
            try
            {
                using var feedIterator = _container.GetItemQueryIterator<T>(query, requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(assetId),
                    MaxBufferedItemCount = 1,
                    MaxItemCount = 1
                });

                if (feedIterator == null || !feedIterator.HasMoreResults) 
                    return new LoadMarketDataResult<T>{ Success = false, Message = "Market data not found."};

-               var response = await feedIterator.ReadNextAsync();
-               return new LoadMarketDataResult<T> { Success = true, Result = response.FirstOrDefault() };
+               var response = await feedIterator.ReadNextAsync();
+               var item = response.FirstOrDefault();
+               return new LoadMarketDataResult<T>
+               {
+                   Success = item != null,
+                   Result  = item,
+                   Message = item == null ? "Market data not found." : null
+               };
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
        public required bool Success { get; init; }

        public string? Id { get; init; } = string.Empty;
        
        public int? Version { get; init; }

        public Exception? Exception { get; init; }
        
        public string? Message { get; init; } = string.Empty;
    }

    public class LoadMarketDataResult<T> where T : IMarketData
    {
        public required bool Success { get; init; }

        public T? Result { get; init; }
        
        public Exception? Exception { get; init; }
        
        public string? Message { get; init; } = string.Empty;
    }
}

public static class MarketDataRepositoryExtensions
{
    /// <summary>
    /// Retrieves the most recent document (by asOfDate) of a specific type for the provided asset and region
    /// by querying the configured Cosmos DB container. The document is determined based on the
    /// latest as-of date and version number.
    /// </summary>
    /// <typeparam name="T">The type of the market data object, implementing IMarketData interface.</typeparam>
    /// <param name="repository">The repository instance used to fetch the document.</param>
    /// <param name="dataType">The data type of the document to retrieve.</param>
    /// <param name="assetClass">The asset class associated with the document.</param>
    /// <param name="assetId">The unique identifier of the asset associated with the document.</param>
    /// <param name="region">The region associated with the document.</param>
    /// <param name="documentType">The type of document to retrieve.</param>
    /// <returns>A task representing the asynchronous operation, returning the result
    /// that contains the document or an error if the operation was unsuccessful.</returns>
    public static async Task<LoadMarketDataResult<T>> GetMarketDataByMostRecentDate<T>(this MarketDataRepository repository,
        string dataType, string assetClass, string assetId, string region, string documentType)
        where T : IMarketData
    {
        var query = new QueryDefinition(
                "SELECT TOP 1 * FROM c WHERE c.assetId = @assetId AND c.assetClass = @assetClass AND c.region = @region AND c.dataType = @dataType AND c.documentType = @documentType ORDER BY c.asOfDate DESC, c.version DESC")
            .WithParameter("@assetId", assetId)
            .WithParameter("@assetClass", assetClass)
            .WithParameter("@region", region)
            .WithParameter("@dataType", dataType)
            .WithParameter("@documentType", documentType);
        
        var result = await repository.ExecuteMarketDataFetchQuery<T>(assetId, query);
        return result;
    }
}