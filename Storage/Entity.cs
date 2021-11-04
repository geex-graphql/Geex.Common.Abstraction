using System;
using System.Collections.Generic;

using MediatR;

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;

namespace Geex.Common.Abstraction.Storage
{
    public abstract class Entity : MongoDB.Entities.Entity, IModifiedOn
    {
        private Queue<INotification> _domainEvents = new();
        public DateTimeOffset ModifiedOn { get; set; }

        public void AddDomainEvent(params INotification[] events)
        {
            foreach (var @event in events)
            {
                this.DomainEvents.Enqueue(@event);
            }
        }
        // bug:不知道基于什么原因, 字段值(即便是readonly也)会在序列化时被修改成null, 这里强行容错一下
        [BsonIgnore]
        public Queue<INotification> DomainEvents => _domainEvents ??= new Queue<INotification>();
    }
}
