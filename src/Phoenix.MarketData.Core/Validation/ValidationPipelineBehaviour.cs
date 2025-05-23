using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;

namespace Phoenix.MarketData.Domain.Validation;

/// <summary>
/// Pipeline behavior that validates requests using FluentValidation before they are processed
/// </summary>
/// <typeparam name="TRequest">Type of request being handled</typeparam>
/// <typeparam name="TResponse">Type of response being returned</typeparam>
public class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<FluentValidation.IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the ValidationPipelineBehavior class
    /// </summary>
    /// <param name="validators">Collection of validators for the request type</param>
    public ValidationPipelineBehavior(IEnumerable<FluentValidation.IValidator<TRequest>> validators)
    {
        _validators = validators ?? Enumerable.Empty<FluentValidation.IValidator<TRequest>>();
    }

    /// <summary>
    /// Handles the pipeline step by validating the request before passing it to the next handler
    /// </summary>
    /// <param name="request">The request being handled</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the next handler</returns>
    /// <exception cref="ValidationException">Thrown when validation fails</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            // Skip validation if there are no validators
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        // Execute all validators asynchronously
        var validationTasks = _validators
            .Select(v => v.ValidateAsync(context, cancellationToken));

        // Wait for all validation tasks to complete
        var validationResults = await Task.WhenAll(validationTasks);

        // Collect all failures
        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        if (failures.Any())
        {
            // Transform FluentValidation failures to our ValidationError format
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

        // Continue with the pipeline if validation passes
        return await next();
    }
}