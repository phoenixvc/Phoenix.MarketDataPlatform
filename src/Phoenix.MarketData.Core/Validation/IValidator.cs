using System.Threading.Tasks;
using System.Threading;

namespace Phoenix.MarketData.Domain.Validation
{
    /// <summary>
    /// Interface for validators that can validate entities
    /// </summary>
    /// <typeparam name="T">The type of entity to validate</typeparam>
    public interface IValidator<T>
    {
        /// <summary>
        /// Validates an entity and returns the validation result
        /// </summary>
        /// <param name="entity">The entity to validate</param>
        /// <returns>A validation result indicating success or failure with error details</returns>
        Task<ValidationResult> ValidateAsync(T entity, CancellationToken cancellationToken = default);
    }
}