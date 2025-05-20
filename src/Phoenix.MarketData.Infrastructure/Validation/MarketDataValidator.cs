using System;
using System.Collections.Generic;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Core.Validation;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Validation
{
    public interface IValidator { }
    public interface IValidator<T> : IValidator where T : IMarketDataEntity
    {
        void Validate(T marketData);
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public class MarketDataValidator : IMarketDataValidator
    {
        private readonly Dictionary<Type, IValidator> _validators = new();

        public MarketDataValidator()
        {
            _validators.Add(typeof(FxSpotPriceData), new FxSpotPriceDataValidator());
            _validators.Add(typeof(CryptoOrdinalSpotPriceData), new CryptoOrdinalSpotPriceDataValidator());
            // Register others here
        }

        public void ValidateMarketData<T>(T marketData) where T : IMarketDataEntity
        {
            if (marketData == null)
                throw new ArgumentNullException(nameof(marketData));
            if (string.IsNullOrWhiteSpace(marketData.AssetId))
                throw new ValidationException("AssetId cannot be null or empty");
            if (string.IsNullOrWhiteSpace(marketData.AssetClass))
                throw new ValidationException("AssetClass cannot be null or empty");
            if (string.IsNullOrWhiteSpace(marketData.DataType))
                throw new ValidationException("DataType cannot be null or empty");
            if (string.IsNullOrWhiteSpace(marketData.DocumentType))
                throw new ValidationException("DocumentType cannot be null or empty");

            if (_validators.TryGetValue(typeof(T), out var validator))
            {
                ((IValidator<T>)validator).Validate(marketData);
            }
            else
            {
                throw new ValidationException($"No validator registered for type {typeof(T).Name}");
            }
        }
    }
}
