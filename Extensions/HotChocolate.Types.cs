using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Geex.Common;
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

        public static IFilterFieldDescriptor PostFilterField<T, TField>(
            this IFilterInputTypeDescriptor<T> @this,
            Expression<Func<T, TField>> property)
        {
            // <TField>(Expression<Func<T, TField>>
            var field = @this.Field<TField>(property);
            var prop = ((property.Body as MemberExpression).Member as PropertyInfo);
            GeexQueryablePostFilterProvider.PostFilterFields.Add(prop.GetHashCode(),prop);
            return field;
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


        public static string GetAggregateAuthorizePrefix<TAggregate>(this IObjectTypeDescriptor<TAggregate> @this) {

            var moduleName = typeof(TAggregate).Name.RemovePreFix("I").ToCamelCase();
            var prefix = $"query.{moduleName}";
            return prefix;
        }

        public static IObjectFieldDescriptor AuthorizeWithDefaultName(this IObjectFieldDescriptor @this) {
            
            var  trace = new StackTrace();
            //获取是哪个类来调用的
            var className = trace.GetFrame(1).GetMethod().DeclaringType.Name;
            var result = "";
            if (className.Contains("Query")) {
                result = $"{className.Replace("Query","").ToCamelCase()}.query";

            } else if (className.Contains("Mutation")) {
                result = $"{className.Replace("Mutation", "").ToCamelCase()}.mutation";

            }  else if (className.Contains("Subscription")) {
                result = $"{className.Replace("Subscription", "").ToCamelCase()}.subscription";
            }
            return @this.Authorize(result);
        }

        public static IObjectFieldDescriptor FieldWithDefaultAuthorize<T,TValue>(this IObjectTypeDescriptor<T>  @this,Expression<Func<T, TValue>> propertyOrMethod)
        {
            return @this.FieldWithDefaultAuthorize((propertyOrMethod.Body as MemberExpression)!.Member);
        }

        public static IObjectFieldDescriptor FieldWithDefaultAuthorize<T>(this IObjectTypeDescriptor<T> @this, MemberInfo propertyOrMethod)
        {
            var prefix = @this.GetAggregateAuthorizePrefix();
            return @this.Field(propertyOrMethod).Authorize($"{prefix}.{propertyOrMethod.Name.ToCamelCase()}");
        }
    }
}
