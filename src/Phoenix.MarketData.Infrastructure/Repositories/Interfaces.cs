using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync(bool includeSoftDeleted = false);
        Task<(IEnumerable<T> Items, string? ContinuationToken)> GetPagedAsync(int pageSize, string? continuationToken = null, bool includeSoftDeleted = false);
        Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, bool includeSoftDeleted = false);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(string id, bool soft = false);
        Task<int> BulkInsertAsync(IEnumerable<T> entities);
    }
}