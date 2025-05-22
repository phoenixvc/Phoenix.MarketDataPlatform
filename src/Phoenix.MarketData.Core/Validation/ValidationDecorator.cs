using System;
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
    /// <typeparam name="TRequest">Type of service being decorated</typeparam>
    public class ValidationDecorator<TRequest> : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        private readonly IRequestHandler<TRequest> _decoratedHandler;
        private readonly IValidator<TRequest>? _validator;

        /// <summary>
        /// Initializes a new instance of the ValidationDecorator class
        /// </summary>
        /// <param name="decoratedHandler">The handler being decorated</param>
        /// <param name="validator">The validator to use (can be null to skip validation)</param>
        /// <exception cref="ArgumentNullException">Thrown when decoratedHandler is null</exception>
        public ValidationDecorator(
            IRequestHandler<TRequest> decoratedHandler,
            IValidator<TRequest>? validator = null)
        {
            _decoratedHandler = decoratedHandler ?? throw new ArgumentNullException(nameof(decoratedHandler));
            _validator = validator; // Null validator is allowed (means no validation will be performed)
        }

        /// <summary>
        /// Handles the request by validating it first, then passing it to the decorated handler
        /// </summary>
        /// <param name="request">The request to handle</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
        public async Task Handle(TRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            await ValidateRequestAsync(request, cancellationToken);
            await _decoratedHandler.Handle(request, cancellationToken);
        }

        /// <summary>
        /// Validates the request using the configured validator and throws a ValidationException if validation fails
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task that completes when validation is done</returns>
        /// <exception cref="ValidationException">Thrown when validation fails</exception>
        private async Task ValidateRequestAsync(TRequest request, CancellationToken cancellationToken)
        {
            if (_validator == null)
                return; // No validation needed if validator is null

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
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