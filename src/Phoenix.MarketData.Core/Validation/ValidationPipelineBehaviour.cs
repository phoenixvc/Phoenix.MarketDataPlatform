using MediatR;
using FluentValidation;

namespace Phoenix.MarketData.Core.Validation;

// No: public interface IValidator<T> {...}  <-- do not shadow FluentValidation here

public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<FluentValidation.IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<FluentValidation.IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(
                failures.Select(f => new ValidationError
                {
                    PropertyName = f.PropertyName,
                    ErrorMessage = f.ErrorMessage,
                    ErrorCode = f.ErrorCode,
                    Source = "FluentValidation"
                })
            );
        }

        return await next();
    }

}
