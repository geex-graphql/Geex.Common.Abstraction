using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using HotChocolate;

using MediatR;

namespace Geex.Common.Abstraction.Auditing
{
    public interface IHasAuditMutation
    {
    }
    public interface IHasAuditMutation<T> : IHasAuditMutation
    {
        async Task<bool> SubmitAsync([Service] IMediator mediator, string[] ids, string? remark)
        {
            await mediator.Send(new SubmitRequest<T>(remark, ids));
            return true;
        }

        async Task<bool> AuditAsync([Service] IMediator mediator, string[] ids, string? remark)
        {
            await mediator.Send(new AuditRequest<T>(remark, ids));
            return true;
        }
        async Task<bool> UnsubmitAsync([Service] IMediator mediator, string[] ids, string? remark)
        {
            await mediator.Send(new UnsubmitRequest<T>(remark, ids));
            return true;
        }

        async Task<bool> UnauditAsync([Service] IMediator mediator, string[] ids, string? remark)
        {
            await mediator.Send(new UnauditRequest<T>(remark, ids));
            return true;
        }
    }
}
