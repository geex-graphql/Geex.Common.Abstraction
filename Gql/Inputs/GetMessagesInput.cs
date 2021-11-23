using System;
using System.Linq;
using System.Linq.Expressions;

using HotChocolate;

using MediatR;

namespace Geex.Common.Abstraction.Gql.Inputs
{
    public class QueryInput<T> : IRequest<IQueryable<T>>
    {
        public QueryInput(Expression<Func<T, bool>> filter = default)
        {
            this.Filter = filter;
        }
        [GraphQLIgnore]
        public Expression<Func<T, bool>> Filter { get; set; }

        public string? _ { get; set; }
    }
}
