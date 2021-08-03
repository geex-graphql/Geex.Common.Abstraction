using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
        async Task<bool> SubmitAsync([Service] IMediator mediator, string id)
        {
            await mediator.Send(new SubmitRequest<T>(id));
            return true;
        }

        async Task<bool> AuditAsync([Service] IMediator mediator, string id)
        {
            await mediator.Send(new AuditRequest<T>(id));
            return true;
        }
    }

    public class SubmitRequest<T> : IRequest<Unit>
    {
        public SubmitRequest(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }
    }

    public class AuditRequest<T> : IRequest<Unit>
    {
        public AuditRequest(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }
    }

    public interface IAuditRequestHandler<TInterface, TEntity> : IRequestHandler<SubmitRequest<TInterface>>, IRequestHandler<AuditRequest<TInterface>> where TInterface : IAuditEntity where TEntity : TInterface
    {
        public Func<string, Task<TEntity>> EntityFinder { get; }

        async Task<Unit> IRequestHandler<SubmitRequest<TInterface>, Unit>.Handle(SubmitRequest<TInterface> request, CancellationToken cancellationToken)
        {
            var entity = await EntityFinder.Invoke(request.Id);
            await (entity).SubmitAsync<TInterface>();
            await entity.SaveAsync(cancellationToken);
            return Unit.Value;
        }

        async Task<Unit> IRequestHandler<AuditRequest<TInterface>, Unit>.Handle(AuditRequest<TInterface> request, CancellationToken cancellationToken)
        {
            var entity = await EntityFinder.Invoke(request.Id);
            await (entity).AuditAsync<TInterface>();
            await entity.SaveAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
