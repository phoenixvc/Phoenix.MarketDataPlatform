using System;

namespace Phoenix.MarketData.Infrastructure.Repositories
{
    // Exceptions related to repository operations
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string entityName, string id)
            : base($"{entityName} with ID '{id}' was not found.")
        {
        }
    }

    public class EntityAlreadyExistsException : Exception
    {
        public EntityAlreadyExistsException(string entityName, string id)
            : base($"{entityName} with ID '{id}' already exists.")
        {
        }
    }

    public class RepositoryOperationException : Exception
    {
        public RepositoryOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}