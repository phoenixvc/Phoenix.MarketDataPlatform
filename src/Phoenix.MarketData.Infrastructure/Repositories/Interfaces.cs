using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Core.Events;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Repositories
{
    // ========== CORE INTERFACES ==========
    public interface ISoftDeletable { bool IsDeleted { get; set; } }

    public interface IRepository<T> where T : class, IEntity
    {
        Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(bool includeSoftDeleted = false, CancellationToken cancellationToken = default);
        Task<(IEnumerable<T> Items, string? ContinuationToken)> GetPagedAsync(int pageSize, string? continuationToken = null, bool includeSoftDeleted = false, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, bool includeSoftDeleted = false, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, bool soft = false, CancellationToken cancellationToken = default);
        Task<int> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // Special market data methods
        Task<T?> GetLatestMarketDataAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType,
            CancellationToken cancellationToken = default);

        Task<int> GetNextVersionAsync(
            string dataType, string assetClass, string assetId, string region,
            DateOnly asOfDate, string documentType,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> QueryByRangeAsync(
            string dataType,
            string assetClass,
            string? assetId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetAllVersionsAsync(string baseId, CancellationToken cancellationToken = default);

        Task<int> PurgeSoftDeletedAsync(CancellationToken cancellationToken = default);
    }
}