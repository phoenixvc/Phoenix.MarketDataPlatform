using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Core.Events;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Repositories
{
     // ========== EVENTS ==========
    public class EntityCreatedEvent<T>
    {
        public T Entity { get; }
        public DateTimeOffset Timestamp { get; }
        public EntityCreatedEvent(T entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            Timestamp = DateTimeOffset.UtcNow;
        }
    }
    public class EntityUpdatedEvent<T>
    {
        public T Entity { get; }
        public DateTimeOffset Timestamp { get; }
        public EntityUpdatedEvent(T entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            Timestamp = DateTimeOffset.UtcNow;
        }
    }
    public class EntityDeletedEvent
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