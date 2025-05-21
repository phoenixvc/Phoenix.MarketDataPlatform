using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace Phoenix.MarketData.Core.Validation
{
    /// <summary>
    /// Generic decorator for adding validation to any service operation
    /// </summary>
    /// <typeparam name="TService">Type of service being decorated</typeparam>
    public class ValidationDecorator<TRequest> : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        private readonly IRequestHandler<TRequest> _decoratedHandler;
        private readonly IValidator<TRequest> _validator;

        public ValidationDecorator(
            IRequestHandler<TRequest> decoratedHandler,
            IValidator<TRequest> validator)
        {
            _decoratedHandler = decoratedHandler ?? throw new System.ArgumentNullException(nameof(decoratedHandler));
            _validator = validator;
        }

        public async Task Handle(TRequest request, CancellationToken cancellationToken)
        {
            await ValidateAsync(request, _validator, cancellationToken);
            await _decoratedHandler.Handle(request, cancellationToken);
        }

        /// <summary>
        /// Validates an entity using the provided validator and throws a ValidationException if validation fails
        /// </summary>
        /// <typeparam name="TEntity">Type of entity to validate</typeparam>
        /// <param name="entity">The entity to validate</param>
        /// <param name="validator">The validator to use</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task that completes when validation is done</returns>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        private async Task ValidateAsync<TEntity>(TEntity entity, IValidator<TEntity> validator, CancellationToken cancellationToken)
        {
            if (validator == null)
                return;

            var validationResult = await validator.ValidateAsync(entity, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(f => new ValidationError
                {
                    PropertyName = f.PropertyName,
                    ErrorMessage = f.ErrorMessage,
                    ErrorCode = f.ErrorCode,
                    Source = "FluentValidation"
                });

                throw new ValidationException(errors);
            }
        }
    }
}
