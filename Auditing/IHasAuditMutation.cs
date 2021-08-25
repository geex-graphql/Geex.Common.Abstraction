using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

using HotChocolate;

using MediatR;

using MongoDB.Entities;

namespace Geex.Common.Abstraction.Auditing
{
    public interface IHasAuditMutation
    {
    }
    public interface IHasAuditMutation<T> : IHasAuditMutation
    {
        async Task<bool> SubmitAsync([Service] IMediator mediator, string[] ids)
        {
            await mediator.Send(new SubmitRequest<T>(ids));
            return true;
        }

        async Task<bool> AuditAsync([Service] IMediator mediator, string[] ids)
        {
            await mediator.Send(new AuditRequest<T>(ids));
            return true;
        }
    }

    public class SubmitRequest<T> : IRequest<Unit>
    {
        public SubmitRequest(string[] sid)
        {
            this.Ids = sid;
        }

        public SubmitRequest(string id)
        {
            this.Ids = new[] { id };
        }

        public string[] Ids { get; set; }
    }

    public class AuditRequest<T> : IRequest<Unit>
    {
        public AuditRequest(string[] ids)
        {
            this.Ids = ids;
        }
        public AuditRequest(string id)
        {
            this.Ids = new[] { id };
        }

        public string[] Ids { get; set; }
    }

    public interface IAuditRequestHandler<TInterface, TEntity> : IRequestHandler<SubmitRequest<TInterface>>, IRequestHandler<AuditRequest<TInterface>> where TInterface : IAuditEntity where TEntity : TInterface
    {
        public DbContext DbContext { get; }

        async Task<Unit> IRequestHandler<SubmitRequest<TInterface>, Unit>.Handle(SubmitRequest<TInterface> request, CancellationToken cancellationToken)
        {
            var entities = DbContext.Queryable<TEntity>().Where(x => request.Ids.Contains(x.Id)).ToList();
            if (!entities.Any())
            {
                throw new BusinessException(GeexExceptionType.NotFound);
            }
            foreach (var entity in entities)
            {
                if (entity is { DbContext: null })
                {
                    DbContext.Attach(entity);
                }
                await entity.SubmitAsync<TInterface>();
            }
            await entities.SaveAsync(cancellation: cancellationToken);
            return Unit.Value;
        }

        async Task<Unit> IRequestHandler<AuditRequest<TInterface>, Unit>.Handle(AuditRequest<TInterface> request, CancellationToken cancellationToken)
        {
            var entities = DbContext.Queryable<TEntity>().Where(x => request.Ids.Contains(x.Id)).ToList();
            if (!entities.Any())
            {
                throw new BusinessException(GeexExceptionType.NotFound);
            }
            foreach (var entity in entities)
            {
                if (entity is { DbContext: null })
                {
                    DbContext.Attach(entity);
                }

                await entity.AuditAsync<TInterface>();
            }

            await entities.SaveAsync(cancellation: cancellationToken);

            return Unit.Value;
        }
    }
}
