using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Phoenix.MarketData.Domain.Models.Interfaces;
using Phoenix.MarketData.Domain.Repositories;

namespace Phoenix.MarketData.Infrastructure.Cosmos
{
    public class Repository<T> : IRepository<T> where T : IMarketData
    {
        private readonly Container _container;
        private readonly VersionManager _versionManager;

        public Repository(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _versionManager = new VersionManager(this); // Only if your version manager needs it; else, inject as needed.
        }

        public async Task<string> SaveAsync(T marketData)
        {
            if (marketData == null) throw new ArgumentNullException(nameof(marketData));
            await _container.CreateItemAsync(marketData, new PartitionKey(marketData.AssetId));
            return marketData.Id;
        }

        public async Task<(T? Result, string? ETag)> GetBySpecifiedVersionAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType, int version)
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

            var iterator = _container.GetItemQueryIterator<T>(query, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(assetId),
                MaxBufferedItemCount = 1,
                MaxItemCount = 1
            });
            var response = await iterator.ReadNextAsync();
            return (response.FirstOrDefault(), response.ETag);
        }

        public async Task<(T? Result, string? ETag)> GetByLatestVersionAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType)
        {
            var query = new QueryDefinition(
                "SELECT TOP 1 * FROM c WHERE c.assetId = @assetId AND c.assetClass = @assetClass AND c.region = @region AND c.dataType = @dataType AND c.documentType = @documentType AND c.asOfDate = @asOfDate ORDER BY c.version DESC")
                .WithParameter("@assetId", assetId.ToLowerInvariant())
                .WithParameter("@assetClass", assetClass.ToLowerInvariant())
                .WithParameter("@region", region.ToLowerInvariant())
                .WithParameter("@dataType", dataType.ToLowerInvariant())
                .WithParameter("@documentType", documentType.ToLowerInvariant())
                .WithParameter("@asOfDate", asOfDate);

            var iterator = _container.GetItemQueryIterator<T>(query, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(assetId),
                MaxBufferedItemCount = 1,
                MaxItemCount = 1
            });
            var response = await iterator.ReadNextAsync();
            return (response.FirstOrDefault(), response.ETag);
        }

        public async Task<IEnumerable<T>> QueryAsync(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            var queryText = "SELECT * FROM c WHERE c.dataType = @dataType AND c.assetClass = @assetClass";
            if (!string.IsNullOrEmpty(assetId))
                queryText += " AND c.assetId = @assetId";
            if (fromDate != null)
                queryText += " AND c.asOfDate >= @fromDate";
            if (toDate != null)
                queryText += " AND c.asOfDate <= @toDate";

            var query = new QueryDefinition(queryText)
                .WithParameter("@dataType", dataType.ToLowerInvariant())
                .WithParameter("@assetClass", assetClass.ToLowerInvariant());

            if (!string.IsNullOrEmpty(assetId))
                query = query.WithParameter("@assetId", assetId.ToLowerInvariant());
            if (fromDate != null)
                query = query.WithParameter("@fromDate", fromDate.Value);
            if (toDate != null)
                query = query.WithParameter("@toDate", toDate.Value);

            var results = new List<T>();
            var iterator = _container.GetItemQueryIterator<T>(query);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }
            return results;
        }
    }
}
