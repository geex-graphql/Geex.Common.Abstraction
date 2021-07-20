using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstraction;

using HotChocolate.Data.Filters;
using HotChocolate.Language;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Descriptors.Definitions;

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
    }
}
