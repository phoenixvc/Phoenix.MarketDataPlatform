using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Core.Services
{
    public interface IMarketDataService
    {
        Task<string> PublishMarketDataAsync<T>(T marketData) where T : IMarketDataEntity;

        Task<T?> GetLatestMarketDataAsync<T>(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType) where T : IMarketDataEntity;

        Task<IEnumerable<T>> QueryMarketDataAsync<T>(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null) where T : IMarketDataEntity;
    }
}