using System;
using System.Collections.Generic;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Domain.Models.Interfaces;
using Phoenix.MarketData.Domain.Validation;
using Phoenix.MarketData.Infrastructure.Schemas;

namespace Phoenix.MarketData.Infrastructure.Validation
{
    public class MarketDataValidator : IMarketDataValidator
    {
        private readonly Dictionary<Type, IValidator> _validators = new();

        public MarketDataValidator()
        {
            // Register validators for each market data type
            _validators.Add(typeof(FxSpotPriceData), new FxSpotPriceDataValidator());
            _validators.Add(typeof(CryptoOrdinalSpotPriceData), new CryptoOrdinalSpotPriceDataValidator());
            // Add other validators as needed
        }

        public void ValidateMarketData<T>(T marketData) where T : IMarketData
        {
            // Basic validation
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

            // Schema version validation
            if (!SchemaVersions.IsSupported(marketData.SchemaVersion))
                throw new ValidationException($"Schema version {marketData.SchemaVersion} is not supported");

            // Type-specific validation
            if (_validators.TryGetValue(typeof(T), out var validator))
            {
                ((IValidator<T>)validator).Validate(marketData);
            }
        }
    }

    public interface IValidator { }

    public interface IValidator<T> : IValidator where T : IMarketData
    {
        void Validate(T marketData);
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}