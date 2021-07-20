using System;

using Geex.Common.Abstraction;
using Geex.Common.Abstraction.Gql;

using HotChocolate.Types;

using MediatR;

using Microsoft.Extensions.DependencyInjection;

namespace Geex.Common.Gql.Roots
{
    public abstract class QueryTypeExtension<T> : ObjectTypeExtension<T> where T : ObjectTypeExtension
    {
        protected override void Configure(IObjectTypeDescriptor<T> descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);
            descriptor.Field("kind").Ignore();
            base.Configure(descriptor);
        }
    }
}
