using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using MediatR;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Entities;

namespace Geex.Common.Abstraction.Storage
{
    public abstract class Entity : MongoDB.Entities.Entity, IModifiedOn, IValidatableObject
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
        // bug:字段值(即便是readonly也)会在序列化时被修改成null, 这里强行容错一下
        public Queue<INotification> DomainEvents => _domainEvents ??= new Queue<INotification>();

        /// <summary>Determines whether the specified object is valid.</summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield return ValidationResult.Success;
    }
}
}
