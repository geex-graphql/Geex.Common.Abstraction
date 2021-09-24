using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using HotChocolate.Types;

namespace Geex.Common
{
    public class GeexQueryablePostFilterProvider : QueryableFilterProvider
    {
        public GeexQueryablePostFilterProvider(
      Action<IFilterProviderDescriptor<QueryableFilterContext>> configure)
      : base(configure)
        {
        }

        public static Dictionary<int, PropertyInfo> PostFilterFields { get; set; } = new Dictionary<int, PropertyInfo>();

        public override void ConfigureField(NameString argumentName, IObjectFieldDescriptor descriptor)
        {
            base.ConfigureField(argumentName, descriptor);
        }

        public override FieldMiddleware CreateExecutor<TEntityType>(NameString argumentName)
        {
            return (FieldMiddleware)(next => (FieldDelegate)(context => ExecuteAsync(next, context)));

            async ValueTask ExecuteAsync(FieldDelegate next, IMiddlewareContext context)
            {
                await next(context).ConfigureAwait(false);
                IInputField inputField = context.Field.Arguments[(string)argumentName];
                IValueNode filterValueNode = !context.LocalContextData.ContainsKey(QueryableFilterProvider.ContextValueNodeKey) || !(context.LocalContextData[QueryableFilterProvider.ContextValueNodeKey] is IValueNode valueNode) ? context.ArgumentLiteral<IValueNode>(argumentName) : valueNode;
                object obj1;
                bool flag1 = context.LocalContextData.TryGetValue(QueryableFilterProvider.SkipFilteringKey, out obj1) && obj1 is bool flag && flag;
                object obj2;
                if (filterValueNode.IsNull() | flag1 || !(inputField.Type is IFilterInputType type) || !context.Field.ContextData.TryGetValue(QueryableFilterProvider.ContextVisitFilterArgumentKey, out obj2) || !(obj2 is VisitFilterArgument visitFilterArgument))
                    return;
                bool inMemory = context.Result is QueryableExecutable<TEntityType> result && result.InMemory || !(context.Result is IQueryable) || context.Result is EnumerableQuery;
                QueryableFilterContext context1 = visitFilterArgument(filterValueNode, type, inMemory);
                //if ((context1.Scopes.FirstOrDefault()?.Level.FirstOrDefault()?.FirstOrDefault() is BinaryExpression binaryExpression) && binaryExpression.Left is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo && !propertyInfo.CanWrite)
                //{

                //}
                Expression<Func<TEntityType, bool>> expression;
                if (context1.TryCreateLambda<Func<TEntityType, bool>>(out expression))
                {
                    IMiddlewareContext middlewareContext = context;
                    object obj3;
                    switch (context.Result)
                    {
                        case IQueryable<TEntityType> source1:
                            var postExpression = default(Expression<Func<TEntityType, bool>>);
                            Expression ProcessUncomputableExpressions(Expression exp, ExpressionType mergeType)
                            {
                                switch (exp.NodeType)
                                {
                                    case ExpressionType.AndAlso:
                                        {
                                            var binary = exp as BinaryExpression;
                                            var left = binary.Left;
                                            var right = binary.Right;
                                            left = ProcessUncomputableExpressions(left, binary.NodeType);
                                            right = ProcessUncomputableExpressions(right, binary.NodeType);
                                            postExpression = Expression.Lambda<Func<TEntityType, bool>>(postExpression == default ? exp : Expression.AndAlso(postExpression.Body, exp), expression.Parameters);
                                            exp = binary.Update(left, binary.Conversion, right);
                                            break;
                                        }
                                    case ExpressionType.OrElse:
                                        {
                                            var binary = exp as BinaryExpression;
                                            var left = binary.Left;
                                            var right = binary.Right;
                                            left = ProcessUncomputableExpressions(left, binary.NodeType);
                                            right = ProcessUncomputableExpressions(right, binary.NodeType);
                                            postExpression = Expression.Lambda<Func<TEntityType, bool>>(postExpression == default ? exp : Expression.OrElse(postExpression.Body, exp), expression.Parameters);
                                            exp = binary.Update(left, binary.Conversion, right);
                                            break;
                                        }
                                    case ExpressionType.Call:
                                        {
                                            var methodCallExp = exp as MethodCallExpression;
                                            if (methodCallExp.Object is not MemberExpression memberExp) return exp;
                                            while (memberExp.Expression is MemberExpression nestedMemberExp)
                                            {
                                                memberExp = nestedMemberExp;
                                            }

                                            if (memberExp.Member is not PropertyInfo propertyInfo) return exp;
                                            if (!PostFilterFields.ContainsKey(propertyInfo.GetHashCode())) return exp;
                                            // 如果是postfilter属性

                                            switch (mergeType)
                                            {
                                                case ExpressionType.OrElse:
                                                    postExpression = Expression.Lambda<Func<TEntityType, bool>>(postExpression == default ? exp : Expression.OrElse(postExpression.Body, exp), expression.Parameters);
                                                    exp = Expression.Constant(true);
                                                    return exp;
                                                default:
                                                    postExpression = Expression.Lambda<Func<TEntityType, bool>>(postExpression == default ? exp : Expression.AndAlso(postExpression.Body, exp), expression.Parameters);
                                                    exp = Expression.Constant(true);
                                                    return exp;
                                            }
                                        }
                                    default:
                                        {
                                            var binary = exp as BinaryExpression;
                                            if (binary.Left is not MemberExpression memberExp) return exp;
                                            while (memberExp.Expression is MemberExpression nestedMemberExp)
                                            {
                                                memberExp = nestedMemberExp;
                                            }

                                            if (memberExp.Member is not PropertyInfo propertyInfo) return exp;
                                            if (!PostFilterFields.ContainsKey(propertyInfo.GetHashCode())) return exp;
                                            // 如果是postfilter属性
                                            switch (mergeType)
                                            {
                                                case ExpressionType.OrElse:
                                                    postExpression = Expression.Lambda<Func<TEntityType, bool>>(postExpression == default ? exp : Expression.OrElse(postExpression.Body, exp), expression.Parameters);
                                                    exp = Expression.Constant(true);
                                                    return exp;
                                                default:
                                                    postExpression = Expression.Lambda<Func<TEntityType, bool>>(postExpression == default ? exp : Expression.AndAlso(postExpression.Body, exp), expression.Parameters);
                                                    exp = Expression.Constant(true);
                                                    return exp;
                                            }
                                        }
                                }
                                return exp;
                            }
                            expression = expression.Update(ProcessUncomputableExpressions(expression.Body, expression.Body.NodeType), expression.Parameters);
                            var data = source1.Where<TEntityType>(expression).ToList().AsQueryable();
                            //bug:默认只支持检查到第一个数据级别
                            if (postExpression != default)
                            {
                                if (postExpression.Body.NodeType == ExpressionType.OrElse && expression.ToString().Contains(expression.Parameters[0].Name + "."))
                                {
                                    throw new NotSupportedException("不支持or运算混用post filter查询");
                                }
                                data = data.Where<TEntityType>(postExpression);
                            }
                            obj3 = data;
                            break;
                        case IEnumerable<TEntityType> source2:
                            obj3 = (object)source2.AsQueryable<TEntityType>().Where<TEntityType>(expression);
                            break;
                        case QueryableExecutable<TEntityType> queryableExecutable:
                            obj3 = (object)queryableExecutable.WithSource(queryableExecutable.Source.Where<TEntityType>(expression));
                            break;
                        default:
                            obj3 = context.Result;
                            break;
                    }
                    middlewareContext.Result = obj3;
                }
                else
                {
                    if (context1.Errors.Count <= 0)
                        return;
                    context.Result = (object)Array.Empty<TEntityType>();
                    foreach (IError error in (IEnumerable<IError>)context1.Errors)
                        context.ReportError(error.WithPath(context.Path));
                }


            }
        }
    }
}