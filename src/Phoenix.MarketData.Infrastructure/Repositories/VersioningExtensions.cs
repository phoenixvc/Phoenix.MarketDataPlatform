using Microsoft.Azure.Cosmos.Linq;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Repositories;

public static class VersioningExtensions
{
    public static async Task<int> GetNextVersionAsync<T>(
        this CosmosRepository<T> repo,
        string dataType,
        string assetClass,
        string assetId,
        string region,
        DateOnly asOfDate,
        string documentType)
        where T : class, IVersionedMarketDataEntity
    {
        var container = repo.GetContainer();
        var query = container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution: false)
            .Where(e =>
                e.AssetId == assetId &&
                e.AssetClass == assetClass &&
                e.Region == region &&
                e.DataType == dataType &&
                e.DocumentType == documentType &&
                e.AsOfDate == asOfDate)
            .OrderByDescending(e => e.Version)
            .Take(1)
            .ToFeedIterator();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            var latest = response.FirstOrDefault();
            if (latest != null)
                return latest.Version + 1;
        }
        return 1;
    }
}
