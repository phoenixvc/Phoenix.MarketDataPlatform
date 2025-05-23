using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Domain.Events;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Repositories
{

    /// <summary>
    /// Base interface for all entity lifecycle events
    /// </summary>
    public interface IEntityEvent
    {
        DateTimeOffset Timestamp { get; }
    }

    /// <summary>
    /// Event raised when a new entity is created in the repository.
    /// </summary>
    /// <typeparam name="T">The type of the entity that was created</typeparam>
    public class EntityCreatedEvent<T> : IEntityEvent
    {
        public T Entity { get; }
        public DateTimeOffset Timestamp { get; }
        public EntityCreatedEvent(T entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            Timestamp = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// Event raised when an entity is updated in the repository.
    /// </summary>
    /// <typeparam name="T">The type of the entity that was updated</typeparam>
    public class EntityUpdatedEvent<T> : IEntityEvent
    {
        public T Entity { get; }
        public DateTimeOffset Timestamp { get; }
        public EntityUpdatedEvent(T entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            Timestamp = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// Event raised when an entity is deleted from the repository.
    /// </summary>
    public class EntityDeletedEvent : IEntityEvent
    {
        public string Id { get; }
        public string EntityType { get; }
        public DateTimeOffset Timestamp { get; }
        public EntityDeletedEvent(string id, string entityType)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            Timestamp = DateTimeOffset.UtcNow;
        }
    }
}