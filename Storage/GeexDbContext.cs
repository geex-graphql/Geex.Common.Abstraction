using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;
using MongoDB.Entities;

using MoreLinq;

using Volo.Abp;

using BusinessException = Geex.Common.Abstractions.BusinessException;

namespace Geex.Common.Abstraction.Storage
{
    public class GeexDbContext : DbContext
    {
        public GeexDbContext(IServiceProvider serviceProvider = default, string database = default,
            bool transactional = false,
            ClientSessionOptions options = null) : base(serviceProvider, database, transactional, options)
        {

        }

        public override T Attach<T>(T entity)
        {
            Check.NotNull(entity, nameof(entity));
            if (entity is Entity geexEntity && entity.Id.IsNullOrEmpty())
            {
                this.DomainEvents.Enqueue(new EntityCreatedNotification<T>((T)(object)geexEntity));
            }

            return base.Attach(entity);
        }

        public override IEnumerable<T> Attach<T>(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                this.Attach(entity);
            }

            return entities;
        }
        public Queue<INotification> DomainEvents { get; } = new Queue<INotification>();

        /// <summary>
        /// Commits a transaction to MongoDB
        /// </summary>
        /// <param name="cancellation">An optional cancellation token</param>
        public override async Task CommitAsync(CancellationToken cancellation = default)
        {

            var mediator = ServiceProvider.GetService<IMediator>();
            if (this.DomainEvents.Any())
            {
                while (this.DomainEvents.TryDequeue(out var @event))
                {
                    await mediator?.Publish(@event, cancellation);
                }
            }

            var entities = Local.TypedCacheDictionary.Values.SelectMany(y => y.Values).OfType<Entity>();
            foreach (var entity in entities)
            {
                var validateResult = entity.Validate(new ValidationContext(entity, ServiceProvider, null));
                if (validateResult.Any(x => x != ValidationResult.Success))
                {
                    throw new BusinessException(GeexExceptionType.ValidationFailed, null,
                        string.Join("\\\n", validateResult.Select(x => x.ErrorMessage)));
                }
            }

            await base.CommitAsync(cancellation);
        }
    }
}
