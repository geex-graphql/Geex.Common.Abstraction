using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    }
}
