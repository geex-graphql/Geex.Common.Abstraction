using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    public static class LinqExtension
    {
        ///// <summary>
        ///// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        ///// </summary>
        ///// <param name="query">Queryable to apply filtering</param>
        ///// <param name="condition">A boolean value</param>
        ///// <param name="predicate">Predicate to filter the query</param>
        ///// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        //public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        //{
        //    return condition
        //        ? query.Where(predicate)
        //        : query;
        //}

        ///// <summary>
        ///// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        ///// </summary>
        ///// <param name="query">Queryable to apply filtering</param>
        ///// <param name="condition">A boolean value</param>
        ///// <param name="predicate">Predicate to filter the query</param>
        ///// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        //public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, int, bool>> predicate)
        //{
        //    return condition
        //        ? query.Where(predicate)
        //        : query;
        //}

    }
}
