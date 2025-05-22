using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Phoenix.MarketData.Core.Validation
{
    /// <summary>
    /// Exception thrown when validation fails
    /// </summary>
    [Serializable]
    public class ValidationException : Exception
    {
        /// <summary>
        /// Gets the collection of validation errors that caused this exception
        /// </summary>
        public IReadOnlyList<ValidationError> Errors { get; }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a collection of validation errors
        /// </summary>
        /// <param name="errors">The validation errors, or null for an empty collection</param>
        public ValidationException(IEnumerable<ValidationError>? errors)
            : base(CreateMessage(errors))
        {
            // Ensure we always have a non-null collection of errors
            Errors = errors?.ToList().AsReadOnly() ?? new ReadOnlyCollection<ValidationError>(Array.Empty<ValidationError>());
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a single validation error
        /// </summary>
        /// <param name="propertyName">The name of the property that failed validation</param>
        /// <param name="errorMessage">The validation error message</param>
        /// <param name="errorCode">Optional error code</param>
        /// <param name="source">Optional source of the validation error</param>
        public ValidationException(string propertyName, string errorMessage, string? errorCode = null, string? source = null)
            : this(new[] { new ValidationError
                {
                    PropertyName = propertyName,
                    ErrorMessage = errorMessage,
                    ErrorCode = errorCode,
                    Source = source
                }
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with a specified error message
        /// </summary>
        /// <param name="message">The error message</param>
        public ValidationException(string message)
            : base(message)
        {
            Errors = Array.Empty<ValidationError>();
        }

        /// <summary>
        /// Initializes a new instance of the ValidationException class with serialized data
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var errorsData = info.GetValue("Errors", typeof(ValidationError[])) as ValidationError[];
            Errors = errorsData ?? Array.Empty<ValidationError>();
        }

        /// <summary>
        /// Gets object data for serialization
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
            info.AddValue("Errors", Errors.ToArray());
        }

        /// <summary>
        /// Creates a validation exception message from a collection of validation errors
        /// </summary>
        /// <param name="errors">The validation errors</param>
        /// <returns>A formatted error message</returns>
        private static string CreateMessage(IEnumerable<ValidationError>? errors)
        {
            if (errors == null || !errors.Any())
                return "Validation failed.";

            var messages = errors.Select(e =>
                string.IsNullOrWhiteSpace(e.PropertyName)
                    ? e.ErrorMessage
                    : $"{e.PropertyName}: {e.ErrorMessage}");

            return "Validation failed: " + string.Join("; ", messages);
        }
    }
}