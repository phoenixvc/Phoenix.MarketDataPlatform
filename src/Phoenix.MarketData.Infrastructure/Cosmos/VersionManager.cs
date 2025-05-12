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

        public async Task<string> GetNextVersionAsync<T>(string dataType, string assetClass, string assetId,
            string region, DateOnly asOfDate, string documentType) where T : IMarketDataObject
        {
            var latest = await _repository.GetLatestAsync<T>(dataType, assetClass, assetId, region, asOfDate, documentType);
            if (latest == null || string.IsNullOrWhiteSpace(latest.Version))
                return "1";

            if (int.TryParse(latest.Version, out var latestVersion))
                return (latestVersion + 1).ToString();

            throw new Exception($"Invalid version format on existing document for {assetId}.");
        }
    }
}