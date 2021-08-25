using System;
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
            descriptor.Field(x=>x.Kind).Ignore();
            descriptor.Field(x=>x.Scope).Ignore();
            descriptor.Field(x=>x.Name).Ignore();
            descriptor.Field(x=>x.Description).Ignore();
            descriptor.Field(x=>x.ContextData).Ignore();
            if (typeof(T).IsAssignableTo<IHasAuditMutation>())
            {
                var mutationType = this.GetType().GetInterface("IHasAuditMutation`1");
                var entityType = mutationType.GenericTypeArguments[0];
                var submit = mutationType.GetMethod(nameof(IHasAuditMutation<IAuditEntity>.SubmitAsync));
                var audit = mutationType.GetMethod(nameof(IHasAuditMutation<IAuditEntity>.AuditAsync));
                descriptor.Field("submit" + entityType.Name.RemovePreFix("I"))
                    .Type<BooleanType>()
                    .Argument("ids", argumentDescriptor => argumentDescriptor.Type(typeof(string[])))
                    .Resolve(resolver: async (context, token) =>
                    {
                        return await (submit.Invoke(this,
                            new object?[] { context.Service<IMediator>(), context.ArgumentValue<string[]>("ids") }) as Task<bool>);
                    })
                    ;
                descriptor.Field("audit" + entityType.Name.RemovePreFix("I"))
                    .Type<BooleanType>()
                    .Argument("ids", argumentDescriptor => argumentDescriptor.Type(typeof(string[])))
                    .Resolve(resolver: async (context, token) =>
                    {
                        return await (audit.Invoke(this,
                            new object?[] { context.Service<IMediator>(), context.ArgumentValue<string[]>("ids") }) as Task<bool>);
                    });
            }
            base.Configure(descriptor);
        }
    }
}
