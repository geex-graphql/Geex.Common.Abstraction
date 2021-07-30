using System;

using Geex.Common.Abstraction;
using Geex.Common.Abstraction.Gql;

using HotChocolate.Types;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Geex.Common.Gql.Roots
{
    public abstract class SubscriptionTypeExtension<T> : ObjectTypeExtension<T> where T : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor<T> descriptor)
        {
            descriptor.Name(OperationTypeNames.Subscription);
            descriptor.Field(x=>x.Kind).Ignore();
            descriptor.Field(x=>x.Scope).Ignore();
            descriptor.Field(x=>x.Name).Ignore();
            descriptor.Field(x=>x.Description).Ignore();
            descriptor.Field(x=>x.ContextData).Ignore();
            base.Configure(descriptor);
        }
    }
}
