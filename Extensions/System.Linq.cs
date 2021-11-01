using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    public static class LinqExtension
    {
        public static string FindCommonPrefix(this string str, params string[] more)
        {
            var prefixLength = str
                              .TakeWhile((c, i) => more.All(s => i < s.Length && s[i] == c))
                              .Count();

            return str[..prefixLength];
        }
        //public static List<TSource> ToList<TSource>(this IQueryable<TSource> source)
        //{
        //    if (source == null)
        //        throw new ArgumentNullException(nameof(source));
        //    return !(source is IIListProvider<TSource> ilistProvider) ? new List<TSource>(source) : ilistProvider.ToList();
        //}

        public static IEnumerable<TResult> Cast<TEnum, TResult>(this IEnumerable<TEnum> source) where TEnum : Enumeration<TEnum, TResult> where TResult : IEquatable<TResult>, IComparable<TResult>
        {
            if (source is IEnumerable<TResult> results)
                return results;
            if (source == null)
                throw new ArgumentNullException("source");
            return source.Select(x => (TResult)x);
        }

        public static IQueryable<TEntityType> WhereWithPostFilter<TEntityType>(this IQueryable<TEntityType> source, Expression<Func<TEntityType, bool>> expression)
        {
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
                            // bug:这里只处理了实例方法调用, 可能需要处理扩展方法
                            if (methodCallExp.Object is not MemberExpression memberExp) return exp;
                            while (memberExp.Expression is MemberExpression nestedMemberExp)
                            {
                                memberExp = nestedMemberExp;
                            }

                            if (memberExp.Member is not PropertyInfo propertyInfo) return exp;
                            //if (!PostFilterFields.ContainsKey(propertyInfo.GetHashCode())) return exp;
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
                            //if (!PostFilterFields.ContainsKey(propertyInfo.GetHashCode())) return exp;
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
            var data = source.Where<TEntityType>(expression).ToList().AsQueryable();
            if (postExpression != default)
            {
                data = data.Where<TEntityType>(postExpression);
            }

            return data;
        }

        public static Expression<Func<T, bool>> CastParamType<T>(this LambdaExpression originExpression)
        {
            //parameter that will be used in generated expression
            var param = Expression.Parameter(typeof(T));
            //visiting body of original expression that gives us body of the new expression
            var body = new TypeCastVisitor<T>(param).Visit((originExpression.Body));
            //generating lambda expression form body and parameter
            //notice that this is what you need to invoke the Method_2
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(body, param);
            return lambda;
        }
    }

    class TypeCastVisitor<T> : ExpressionVisitor
    {
        ParameterExpression _parameter;

        //there must be only one instance of parameter expression for each parameter
        //there is one so one passed here
        public TypeCastVisitor(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        //this method replaces original parameter with given in constructor
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }

        //this one is required because PersonData does not implement IPerson and it finds
        //property in PersonData with the same name as the one referenced in expression
        //and declared on IPerson
        protected override Expression VisitMember(MemberExpression node)
        {
            //only properties are allowed if you use fields then you need to extend
            // this method to handle them
            if (node.Member.MemberType != System.Reflection.MemberTypes.Property)
                return node;

            //name of a member referenced in original expression in your
            //sample Id in mine Prop
            var memberName = node.Member.Name;
            //find property on type T (=PersonData) by name
            var otherMember = typeof(T).GetProperty(memberName);
            //visit left side of this expression p.Id this would be p
            var inner = Visit(node.Expression);
            return Expression.Property(inner, otherMember);
        }
    }
}
