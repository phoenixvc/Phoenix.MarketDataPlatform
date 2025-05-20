using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace Phoenix.MarketData.Core.Validation
{
    public class ValidationCommandHandlerDecorator<TRequest> : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        private readonly IRequestHandler<TRequest> _decorated;
        private readonly IValidator<TRequest> _validator;

        public ValidationCommandHandlerDecorator(
            IRequestHandler<TRequest> decorated,
            IValidator<TRequest> validator)
        {
            _decorated = decorated;
            _validator = validator;
        }

        public async Task Handle(TRequest request, CancellationToken cancellationToken)
        {
            var fvResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!fvResult.IsValid)
            {
                var errors = fvResult.Errors.Select(f => new ValidationError
                {
                    PropertyName = f.PropertyName,
                    ErrorMessage = f.ErrorMessage,
                    ErrorCode = f.ErrorCode,
                    Source = "FluentValidation"
                });

                throw new ValidationException(errors);
            }

            await _decorated.Handle(request, cancellationToken);
        }
    }
}
