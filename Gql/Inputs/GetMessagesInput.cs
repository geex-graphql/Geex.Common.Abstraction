using System.Linq;
using MediatR;

namespace Geex.Common.Abstraction.Gql.Inputs
{
    public class QueryInput<T> : IRequest<IQueryable<T>>
    {
        public string _ { get; set; }
    }
}
