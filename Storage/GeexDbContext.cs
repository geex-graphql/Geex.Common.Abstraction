using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Driver;
using MongoDB.Entities;

using Volo.Abp;

namespace Geex.Common.Abstraction.Storage
{
    public class GeexDbContext : DbContext
    {
        public GeexDbContext(IServiceProvider serviceProvider = default, string database = default, bool transactional = false,
            ClientSessionOptions options = null) : base(serviceProvider, database, transactional, options)
        {

        }
        public override T Attach<T>(T entity)
        {
            Check.NotNull(entity, nameof(entity));
            if (entity is Entity geexEntity && entity.Id.IsNullOrEmpty())
            {
                geexEntity.DomainEvents.Enqueue(new EntityCreatedNotification<T>((T)(object)geexEntity));
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

        /// <summary>
        /// Commits a transaction to MongoDB
        /// </summary>
        /// <param name="cancellation">An optional cancellation token</param>
        public override async Task CommitAsync(CancellationToken cancellation = default)
        {
            var entities = Local.TypedCacheDictionary.Values.SelectMany(y => y.Values).OfType<Entity>();
            var events = entities.Select(entity => entity.DomainEvents).Where(x => x != default);
            foreach (var eventQueue in events)
            {
                while (eventQueue.TryDequeue(out var @event))
                {
                    await ServiceProvider.GetService<IMediator>().Publish(@event, cancellation);
                }
            }

            await base.CommitAsync(cancellation);
        }
    }
}
