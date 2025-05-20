using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Phoenix.MarketData.Core.Validation
{
    public class ValidationException : Exception
    {
        public IReadOnlyList<ValidationError> Errors { get; }

        public ValidationException(IEnumerable<ValidationError> errors)
            : base(CreateMessage(errors))
        {
            Errors = errors?.ToList() ?? new List<ValidationError>();
        }

        private static string CreateMessage(IEnumerable<ValidationError> errors)
        {
            if (errors == null)
                return "Validation failed.";
            var messages = errors.Select(e =>
                string.IsNullOrWhiteSpace(e.PropertyName)
                    ? e.ErrorMessage
                    : $"{e.PropertyName}: {e.ErrorMessage}");
            return "Validation failed: " + string.Join("; ", messages);
        }
    }
}
