using System;
using System.Linq;
using System.Linq.Expressions;

using HotChocolate;

using MediatR;

namespace Geex.Common.Abstraction.Gql.Inputs
{
    public class QueryInput<T> : IRequest<IQueryable<T>>
    {
        public static QueryInput<T> New(Expression<Func<T, bool>> filter = default)
        {
            return new QueryInput<T>(filter);
        }
        public QueryInput(Expression<Func<T, bool>> filter = default)
        {
            this.Filter = filter;
        }
        public QueryInput(params string[] ids)
        {
            if (!typeof(T).IsAssignableTo<IHasId>())
            {
                throw new Exception("id 列表查询仅支持 IHasId");
            }

            this.Filter = x => ids.Contains(((IHasId)x).Id);
            this.Ids = ids;
        }
        [GraphQLIgnore]
        public Expression<Func<T, bool>> Filter { get; set; }
        /// <summary>
        /// 只在做id列表查询的时候有效
        /// </summary>
        [GraphQLIgnore]
        public string[] Ids { get; private set; }
        public string? _ { get; set; }
    }
}
