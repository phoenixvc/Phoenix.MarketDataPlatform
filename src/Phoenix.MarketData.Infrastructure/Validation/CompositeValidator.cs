using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.MarketData.Domain.Validation;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Validation
{
    // This class serves as a composite validator that delegates to type-specific validators
    public class CompositeValidator<T> : IValidator<T> where T : IMarketDataEntity
    {
        // Store type-to-wrapper mappings
        private readonly Dictionary<Type, IValidatorWrapper> _validators = new();

        // Interface for our validator wrappers
        private interface IValidatorWrapper
        {
            Task<ValidationResult> ValidateAsync(object entity, CancellationToken cancellationToken);
        }

        // Generic implementation of the wrapper
        private class ValidatorWrapper<TEntity> : IValidatorWrapper where TEntity : IMarketDataEntity
        {
            private readonly IValidator<TEntity> _validator;

            public ValidatorWrapper(IValidator<TEntity> validator)
            {
                _validator = validator;
            }

            public async Task<ValidationResult> ValidateAsync(object entity, CancellationToken cancellationToken)
            {
                if (entity is TEntity typedEntity)
                {
                    return await _validator.ValidateAsync(typedEntity, cancellationToken);
                }

                throw new InvalidOperationException(
                    $"Expected entity of type {typeof(TEntity).Name}, but got {entity.GetType().Name}");
            }
        }

        public void RegisterValidator<TEntity>(IValidator<TEntity> validator) where TEntity : T
        {
            _validators[typeof(TEntity)] = new ValidatorWrapper<TEntity>(validator);
        }

        public async Task<ValidationResult> ValidateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var entityType = entity.GetType();

            if (_validators.TryGetValue(entityType, out var wrapper))
            {
                return await wrapper.ValidateAsync(entity, cancellationToken);
            }

            // If no specific validator is found, return success
            return ValidationResult.Success();
        }
    }
}