using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Core.Repositories
{
    public interface IRepository<T> where T : IMarketDataEntity
    {
        Task<(T? Result, string? ETag)> GetBySpecifiedVersionAsync(string dataType, string assetClass, string assetId, string region, DateOnly asOfDate, string documentType, int version);
        Task<(T? Result, string? ETag)> GetByLatestVersionAsync(string dataType, string assetClass, string assetId, string region, DateOnly asOfDate, string documentType);
        Task<string> SaveAsync(T marketData);
        Task<IEnumerable<T>> QueryAsync(string dataType, string assetClass, string? assetId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}