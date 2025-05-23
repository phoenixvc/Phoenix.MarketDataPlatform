using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Phoenix.MarketData.Domain.Repositories
{
    /// <summary>
    /// Query-focused repository interface for read operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Entity key type</typeparam>
    public interface IQueryRepository<T, in TKey> where T : class
    {
        /// <summary>
        /// Gets an entity by its unique identifier
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The entity if found; otherwise null</returns>
        Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paged list of all entities
        /// </summary>
        /// <param name="pageIndex">Zero-based page index</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A paged result containing the entities and pagination metadata</returns>
        Task<PagedResult<T>> GetPagedAsync(
            int pageIndex = 0,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        /// <remarks>
        /// Implementations should throw <see cref="ArgumentOutOfRangeException"/> when
        /// <paramref name="pageIndex"/> < 0 or <paramref name="pageSize"/> <= 0.
        /// </remarks>
        /// <summary>
        /// Gets all entities (use with caution on large datasets)
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>All entities in the repository</returns>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries entities based on a predicate with pagination support
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <param name="pageIndex">Zero-based page index</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A paged result containing the filtered entities and pagination metadata</returns>
        Task<PagedResult<T>> QueryPagedAsync(Expression<Func<T, bool>> predicate, int pageIndex = 0, int pageSize = 50, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries entities based on a predicate
        /// </summary>
        /// <param name="predicate">The filter expression</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>Filtered entities</returns>
        Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts entities based on an optional predicate
        /// </summary>
        /// <param name="predicate">The filter expression, or null to count all entities</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The count of entities matching the predicate</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Command-focused repository interface for write operations
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Entity key type</typeparam>
    public interface ICommandRepository<T, in TKey> where T : class
    {
        /// <summary>
        /// Adds a new entity to the repository
        /// </summary>
        /// <param name="entity">The entity to add</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The added entity with any generated values (e.g., ID)</returns>
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity in the repository
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The updated entity</returns>
        Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity by its unique identifier
        /// </summary>
        /// <param name="id">The entity's unique identifier</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>True if the entity was deleted; otherwise false</returns>
        Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts multiple entities in a single operation
        /// </summary>
        /// <param name="entities">The entities to insert</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The inserted entities with any generated values</returns>
        Task<IEnumerable<T>> BulkInsertAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Complete repository interface inheriting both query and command capabilities
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <typeparam name="TKey">Entity key type</typeparam>
    public interface IRepository<T, TKey> : IQueryRepository<T, TKey>, ICommandRepository<T, TKey>
        where T : class
    {
    }

    /// <summary>
    /// Represents a paged result set with metadata
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class PagedResult<T> where T : class
    {
        /// <summary>
        /// Gets or sets the page items
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Gets or sets the total count of available items
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the page index (zero-based)
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets the total page count
        /// </summary>
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

        /// <summary>
        /// Gets whether this is the first page
        /// </summary>
        public bool IsFirstPage => PageIndex == 0;

        /// <summary>
        /// Gets whether this is the last page
        /// </summary>
        public bool IsLastPage => PageIndex >= TotalPages - 1;

        /// <summary>
        /// Gets a token that can be used for continuation in APIs that support it
        /// </summary>
        public string? ContinuationToken { get; set; }
    }
}