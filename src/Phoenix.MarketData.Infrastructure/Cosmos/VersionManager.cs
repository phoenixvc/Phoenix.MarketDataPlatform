using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Infrastructure.Cosmos
{
    public class VersionManager
    {
        private readonly MarketDataRepository _repository;

        public VersionManager(MarketDataRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> GetNextVersionAsync<T>(string dataType, string assetClass, string assetId,
            string region, DateOnly asOfDate, string documentType) where T : IMarketData
        {
            if (string.IsNullOrWhiteSpace(region)) 
            {
                throw new ArgumentException("Region cannot be null or empty", nameof(region));
            }
            
            var latest = await _repository.GetMarketDataByLatestVersionAsync<T>(dataType, assetClass, assetId, region, asOfDate, documentType);
            if (latest.Result == null || latest.Result.Version == null)
                return 1;

            return latest.Result.Version.Value + 1;
        }
    }
}