using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using MediatR;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Entities;


namespace Geex.Common.Abstractions
{
    public abstract class Entity : IEntity, IModifiedOn
    {
        public static ConcurrentQueue<INotification> _domainEvents = new ConcurrentQueue<INotification>();

        [BsonId]
        [ObjectId]
        public string Id { get; set; }

        string IEntity.GenerateNewId()
        {
            return ObjectId.GenerateNewId().ToString();
        }

        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        IClientSessionHandle IEntity.Session { get; set; }

        protected void AddDomainEvent(params INotification[] events)
        {
            foreach (var @event in events)
            {
                _domainEvents.Enqueue(@event);
            }
        }
    }
}
