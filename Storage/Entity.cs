using System;
using System.Collections.Generic;
using MediatR;
using MongoDB.Entities;

namespace Geex.Common.Abstraction.Storage
{
    public abstract class Entity : MongoDB.Entities.Entity, IModifiedOn
    {
        public DateTimeOffset ModifiedOn { get; set; }

        public void AddDomainEvent(params INotification[] events)
        {
            foreach (var @event in events)
            {
                this.DomainEvents.Enqueue(@event);
            }
        }

        public Queue<INotification> DomainEvents { get; } = new();
    }
}
