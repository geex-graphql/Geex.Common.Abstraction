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
using Geex.Common.Authorization;
using Geex.Common.Gql.Types;

using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using HotChocolate.Language;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Descriptors.Definitions;

using Humanizer;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Entities;

using Entity = Geex.Common.Abstraction.Storage.Entity;

// ReSharper disable once CheckNamespace
namespace HotChocolate.Types
{
    public static class Extension
    {
        public static IObjectFieldDescriptor ConfigQuery<TResolver>(
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
            this IObjectTypeDescriptor<T> @this) where T : Entity
        {
            @this.Field(x => x.Id);
            @this.Field(x => x.CreatedOn);
            @this.Field(x => x.Validate(default)).Ignore();
            if (typeof(T).IsAssignableTo<IAuditEntity>())
            {
                @this.Field(x => ((IAuditEntity)x).AuditStatus);
                @this.Field(x => ((IAuditEntity)x).Submittable);
            }
        }

        public static IInterfaceTypeDescriptor<T> IgnoreMethods<T>(
      this IInterfaceTypeDescriptor<T> descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            // specialname过滤属性
            foreach (var method in typeof(T).GetMethods().Where(x => !x.IsSpecialName))
            {
                if (method.ReturnType != typeof(void))
                {
                    var exp = Expression.Lambda(Expression.Convert(Expression.Call(Expression.Parameter(typeof(T), "x"), method, method.GetParameters().Select(x => Expression.Default(x.ParameterType))), typeof(object)), Expression.Parameter(typeof(T), "x"));
                    descriptor.Field(exp as Expression<Func<T, object>>).Ignore();
                }
                //else
                //{
                //    var exp = Expression.Lambda(Expression.Call(Expression.Parameter(typeof(T), "x"), method, method.GetParameters().Select(x => Expression.Default(x.ParameterType))), Expression.Parameter(typeof(T), "x"));
                //    descriptor.Field(exp as Expression<Func<T, object>>).Ignore();
                //}
                //descriptor.Field(method.Name).Ignore();
            }
            return descriptor;
        }

        public static IObjectTypeDescriptor<T> IgnoreMethods<T>(
      this IObjectTypeDescriptor<T> descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            // specialname过滤属性
            foreach (var method in typeof(T).GetMethods().Where(x => !x.IsSpecialName))
            {
                descriptor.Field(method).Ignore();
            }
            return descriptor;
        }

        public static IFilterFieldDescriptor PostFilterField<T, TField>(
            this IFilterInputTypeDescriptor<T> @this,
            Expression<Func<T, TField>> property)
        {
            // <TField>(Expression<Func<T, TField>>
            var field = @this.Field<TField>(property);
            var prop = ((property.Body as MemberExpression).Member as PropertyInfo);
            GeexQueryablePostFilterProvider.PostFilterFields.Add(prop.GetHashCode(), prop);
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
                .BindRuntimeType<ObjectId, ObjectIdType>()
                .BindRuntimeType<dynamic, AnyType>()
                .BindRuntimeType<object, AnyType>();
        }


        public static string GetAggregateAuthorizePrefix<TAggregate>(this IObjectTypeDescriptor<TAggregate> @this)
        {
            var moduleName = typeof(TAggregate).Assembly.GetName().Name.Split(".").ToList().Where(x => !x.IsIn("Gql", "Api", "Core", "Tests")).Last().ToCamelCase();
            var entityName = typeof(TAggregate).Name.ToCamelCase();
            var prefix = $"{moduleName}_query_{entityName}";
            return prefix;
        }

        public static string GetAggregateAuthorizePrefix<TAggregate>(this IInterfaceTypeDescriptor<TAggregate> @this)
        {

            var moduleName = typeof(TAggregate).Assembly.GetName().Name.Split(".").ToList().Where(x => !x.IsIn("Gql", "Api", "Core", "Tests")).Last().ToCamelCase();
            var entityName = typeof(TAggregate).Name.RemovePreFix("I").ToCamelCase();
            var prefix = $"{moduleName}_query_{entityName}";
            return prefix;
        }

        public static IObjectTypeDescriptor<T> AuthorizeWithDefaultName<T>(this IObjectTypeDescriptor<T> @this)
        {
            var trace = new StackTrace();
            //获取是哪个类来调用的
            var caller = trace.GetFrame(1).GetMethod();
            var callerDeclaringType = caller.DeclaringType;
            var moduleName = callerDeclaringType.Assembly.GetName().Name.Split(".").ToList().Where(x => !x.IsIn("Gql", "Api", "Core", "Tests")).Last().ToCamelCase();
            var className = callerDeclaringType.Name;
            var prefix = "";
            if (className.Contains("Query"))
            {
                prefix = $"{moduleName}_query";

            }
            else if (className.Contains("Mutation"))
            {
                prefix = $"{moduleName}_mutation";

            }
            else if (className.Contains("Subscription"))
            {
                prefix = $"{moduleName}_subscription";
            }

            var propertyList = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var item in propertyList)
            {
                var policy = $"{prefix}_{item.Name.ToCamelCase()}";
                if (AppPermission.List.Any(x => x.Value == policy) && AppPermission.List.Any(x => x.Value == policy))
                {
                    @this.Field(item).Authorize(policy);
                    Console.WriteLine($@"成功匹配权限规则:{policy} for {item.DeclaringType.Name}.{item.Name}");
                }
            }

            // 判断是否继承了审核基类
            if (typeof(T).GetInterfaces().Contains(typeof(IHasAuditMutation)))
            {
                var auditMutationType = typeof(T).GetInterfaces().First(x => x.Name.StartsWith(nameof(IHasAuditMutation) + "`1"));
                var auditPropertyList = auditMutationType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                var entityType = auditMutationType.GenericTypeArguments[0];
                foreach (var item in auditPropertyList)
                {
                    var policy = $"{prefix}_{item.Name.ToCamelCase()}{entityType.Name.RemovePreFix("I")}";

                    if (AppPermission.List.Any(x => x.Value == policy) && AppPermission.List.Any(x => x.Value == policy))
                    {
                        // gql版本限制, 重写resolve的字段需要重新指定类型
                        @this.Field(policy.Split('_').Last()).Type<BooleanType>().Authorize(policy);
                        Console.WriteLine($@"成功匹配权限规则:{policy} for {item.DeclaringType.Name}.{item.Name}");
                    }
                }
            }
            return @this;
        }


        public static void AuthorizeFieldsImplicitly<T>(this IObjectTypeDescriptor<T> descriptor) where T : class
        {
            var propertyList = typeof(T).GetProperties();
            foreach (var item in propertyList)
            {
                descriptor.FieldWithDefaultAuthorize(item);
            }
        }


        public static IObjectFieldDescriptor FieldWithDefaultAuthorize<T, TValue>(this IObjectTypeDescriptor<T> @this, Expression<Func<T, TValue>> propertyOrMethod)
        {
            if (propertyOrMethod.Body.NodeType == ExpressionType.Call)
            {
                return @this.FieldWithDefaultAuthorize((propertyOrMethod.Body as MethodCallExpression)!.Method);
            }
            return @this.FieldWithDefaultAuthorize((propertyOrMethod.Body as MemberExpression)!.Member);
        }

        public static IObjectFieldDescriptor FieldWithDefaultAuthorize<T>(this IObjectTypeDescriptor<T> @this, MemberInfo propertyOrMethod)
        {
            var prefix = @this.GetAggregateAuthorizePrefix();
            var fieldDescriptor = @this.Field(propertyOrMethod);
            var policy = $"{prefix}_{propertyOrMethod.Name.ToCamelCase()}";
            if (AppPermission.List.Any(x => x.Value == policy) && AppPermission.List.Any(x => x.Value == policy))
            {
                fieldDescriptor = fieldDescriptor.Authorize(policy);
                Console.WriteLine($@"成功匹配权限规则:{policy} for {propertyOrMethod.DeclaringType.Name}.{propertyOrMethod.Name}");
            }

            return fieldDescriptor;
        }
    }
}
