using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstraction;
using Geex.Common.Abstraction.Auditing;
using Geex.Common.Gql.Types;

using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using HotChocolate.Language;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Descriptors.Definitions;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Entities;

// ReSharper disable once CheckNamespace
namespace HotChocolate.Types
{
    public static class Extension
    {
        public static IObjectFieldDescriptor ResolveMethod<TResolver>(
            this IObjectTypeDescriptor<TResolver> @this,
            Expression<Func<TResolver, object?>> propertyOrMethod)
        {
            var method = (propertyOrMethod.Body as MethodCallExpression).Method;
            var field = @this.Field(method.Name.ToCamelCase());
            foreach (var parameterInfo in method.GetParameters().Where(x => !x.CustomAttributes.Any()))
            {
                field.Argument(parameterInfo.Name, x => x.Type(parameterInfo.ParameterType));
            }
            field.Type(method.ReturnType);
            field = field.ResolveWith(propertyOrMethod);
            return field;
        }

        public static void ConfigEntity<T>(
            this IObjectTypeDescriptor<T> @this) where T : IEntity
        {
            @this.Field(x => x.Id);
            @this.Field(x => x.CreatedOn);
            if (typeof(T).IsAssignableTo<IAuditEntity>())
            {
                @this.Field(x => ((IAuditEntity)x).AuditStatus);
                @this.Field(x => ((IAuditEntity)x).Submittable);
            }
        }

        public static IRequestExecutorBuilder AddCommonTypes(
      this IRequestExecutorBuilder builder)
        {
            return builder
                .AddInterfaceType<IEntity>(x =>
                {
                    x.BindFieldsExplicitly();
                    x.Field(y => y.Id);
                    x.Field(y => y.CreatedOn);
                })
                .AddInterfaceType<IAuditEntity>(x =>
                {
                    x.BindFieldsExplicitly();
                    //x.Implements<IEntityType>();
                    x.Field(y => y.AuditStatus);
                    x.Field(y => y.Submittable);
                })
                .BindRuntimeType<ObjectId, ObjectIdType>();
        }
    }
}
