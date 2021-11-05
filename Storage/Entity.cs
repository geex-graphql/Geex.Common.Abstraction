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
        protected GeexDbContext DbContext
        {
            get => base.DbContext as GeexDbContext;
            set => base.DbContext = value;
        }
        public DateTimeOffset ModifiedOn { get; set; }

        public void AddDomainEvent(params INotification[] events)
        {
            foreach (var @event in events)
            {
                this.DbContext.DomainEvents.Enqueue(@event);
            }
        }

        /// <summary>Determines whether the specified object is valid.</summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield return ValidationResult.Success;
        }
    }
}
