using System;
using System.Linq;
using System.Threading.Tasks;

using Geex.Common.Abstraction;
using Geex.Common.Abstraction.Auditing;
using Geex.Common.Abstraction.Gql;

using HotChocolate.Types;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Entities;

namespace Geex.Common.Gql.Roots
{

    public abstract class MutationTypeExtension<T> : ObjectTypeExtension<T> where T : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor<T> descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);
            descriptor.Field(x => x.Kind).Ignore();
            descriptor.Field(x => x.Scope).Ignore();
            descriptor.Field(x => x.Name).Ignore();
            descriptor.Field(x => x.Description).Ignore();
            descriptor.Field(x => x.ContextData).Ignore();
            if (typeof(T).IsAssignableTo<IHasAuditMutation>())
            {
                var mutationType = this.GetType().GetInterface("IHasAuditMutation`1");
                var entityType = mutationType.GenericTypeArguments[0];

                var submit = mutationType.GetMethod(nameof(IHasAuditMutation<IAuditEntity>.SubmitAsync));
                var audit = mutationType.GetMethod(nameof(IHasAuditMutation<IAuditEntity>.AuditAsync));
                var unsubmit = mutationType.GetMethod(nameof(IHasAuditMutation<IAuditEntity>.UnsubmitAsync));
                var unaudit = mutationType.GetMethod(nameof(IHasAuditMutation<IAuditEntity>.UnauditAsync));
                var submitFieldDescriptor = descriptor.Field("submit" + entityType.Name.RemovePreFix("I"))
                    .Type<BooleanType>()
                    .Argument("ids", argumentDescriptor => argumentDescriptor.Type(typeof(string[])))
                    .Argument("remark", argumentDescriptor => argumentDescriptor.Type(typeof(string)))
                    .Authorize($"mutation.submit{entityType.Name.RemovePreFix("I")}")
                    .Resolve(resolver: async (context, token) =>
                    {
                        return await (submit.Invoke(this,
                            new object?[] { context.Service<IMediator>(), context.ArgumentValue<string[]>("ids"), context.ArgumentValue<string>("remark") }) as Task<bool>);
                    });
                var auditFieldDescriptor = descriptor.Field("audit" + entityType.Name.RemovePreFix("I"))
                    .Type<BooleanType>()
                    .Argument("ids", argumentDescriptor => argumentDescriptor.Type(typeof(string[])))
                    .Argument("remark", argumentDescriptor => argumentDescriptor.Type(typeof(string)))
                    .Authorize($"mutation.audit{entityType.Name.RemovePreFix("I")}")
                    .Resolve(resolver: async (context, token) =>
                    {
                        return await (audit.Invoke(this,
                            new object?[] { context.Service<IMediator>(), context.ArgumentValue<string[]>("ids"), context.ArgumentValue<string>("remark") }) as Task<bool>);
                    });
                var unsubmitFieldDescriptor = descriptor.Field("unsubmit" + entityType.Name.RemovePreFix("I"))
                    .Type<BooleanType>()
                    .Argument("ids", argumentDescriptor => argumentDescriptor.Type(typeof(string[])))
                    .Argument("remark", argumentDescriptor => argumentDescriptor.Type(typeof(string)))
                    .Authorize($"mutation.unsubmit{entityType.Name.RemovePreFix("I")}")
                    .Resolve(resolver: async (context, token) =>
                    {
                        return await (unsubmit.Invoke(this,
                            new object?[] { context.Service<IMediator>(), context.ArgumentValue<string[]>("ids"), context.ArgumentValue<string>("remark") }) as Task<bool>);
                    })
                    ;
                var unauditFieldDescriptor = descriptor.Field("unaudit" + entityType.Name.RemovePreFix("I"))
                    .Type<BooleanType>()
                    .Argument("ids", argumentDescriptor => argumentDescriptor.Type(typeof(string[])))
                    .Argument("remark", argumentDescriptor => argumentDescriptor.Type(typeof(string)))
                    .Authorize($"mutation.unaudit{entityType.Name.RemovePreFix("I")}")
                    .Resolve(resolver: async (context, token) =>
                    {
                        return await (unaudit.Invoke(this,
                            new object?[] { context.Service<IMediator>(), context.ArgumentValue<string[]>("ids"), context.ArgumentValue<string>("remark") }) as Task<bool>);
                    });
            }
            base.Configure(descriptor);
        }
    }
}
