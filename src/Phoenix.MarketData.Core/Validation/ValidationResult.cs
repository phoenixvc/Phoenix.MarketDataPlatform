using System.Collections.Generic;
using System.Linq;

namespace Phoenix.MarketData.Core.Validation
{
    public class ValidationResult
    {
        public bool IsValid => !Errors.Any();
        public IReadOnlyList<ValidationError> Errors { get; }

        public ValidationResult(IEnumerable<ValidationError>? errors = null)
        {
            Errors = errors?.ToList() ?? new List<ValidationError>();
        }

        public static ValidationResult Success() => new ValidationResult();
        public static ValidationResult Failure(IEnumerable<ValidationError> errors) => new ValidationResult(errors);

        public static ValidationResult Failure(string message, string property, string code, string source)
            => new ValidationResult(new[]
            {
            new ValidationError { ErrorMessage = message, PropertyName = property, ErrorCode = code, Source = source }
            });

        public override string ToString()
            => IsValid ? "Valid" : string.Join("; ", Errors.Select(e => e.ToString()));
    }


}
