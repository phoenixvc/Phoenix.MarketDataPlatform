using System;
using System.Threading.Tasks;

namespace Phoenix.MarketData.Domain.Validation
{
    /// <summary>
    /// Generic decorator for adding validation to service operations
    /// </summary>
    /// <typeparam name="TService">Type of service being decorated</typeparam>
    public abstract class ServiceValidationDecorator<TService>
    {
        private readonly TService _decoratedService;

        protected ServiceValidationDecorator(TService service)
        {
            _decoratedService = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Gets the decorated service
        /// </summary>
        protected TService DecoratedService => _decoratedService;

        /// <summary>
        /// Validates an entity using the provided validator and throws a ValidationException if validation fails
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <param name="entity">The entity to validate</param>
        /// <param name="validator">The validator to use</param>
        /// <returns>A task that completes when validation is done</returns>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
        protected async Task ValidateAsync<TEntity>(TEntity entity, IValidator<TEntity> validator)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (validator == null)
                return;

            var validationResult = await validator.ValidateAsync(entity);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}