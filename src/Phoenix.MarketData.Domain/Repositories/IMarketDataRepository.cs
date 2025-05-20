using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Domain.Repositories
{
    public interface IMarketDataRepository
    {
        Task<(T? Result, string? ETag)> GetMarketDataBySpecifiedVersionAsync<T>(
            string dataType, string assetClass, string assetId, string region, 
            DateOnly asOfDate, string documentType, int version) where T : IMarketData;
            
        Task<(T? Result, string? ETag)> GetMarketDataByLatestVersionAsync<T>(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType) where T : IMarketData;
            
        Task<string> SaveMarketDataAsync<T>(T marketData) where T : IMarketData;
        
        Task<IEnumerable<T>> QueryMarketDataAsync<T>(
            string dataType, string assetClass, string? assetId = null,
            DateTime? fromDate = null, DateTime? toDate = null) where T : IMarketData;
    }
}