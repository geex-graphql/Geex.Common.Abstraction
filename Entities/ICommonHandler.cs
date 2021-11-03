using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Geex.Common.Abstraction.Gql.Inputs;
using Geex.Common.Abstraction.Storage;
using Geex.Common.Abstractions;

using MediatR;

using MongoDB.Entities;

namespace Geex.Common.Abstraction.Entities
{
    public interface ICommonHandler<TInterface, TEntity> :
        IRequestHandler<QueryInput<TInterface>, IQueryable<TInterface>>
        where TInterface : IEntity where TEntity : TInterface
    {
        public GeexDbContext DbContext { get; }

        async Task<IQueryable<TInterface>> IRequestHandler<QueryInput<TInterface>, IQueryable<TInterface>>.Handle(QueryInput<TInterface> request, CancellationToken cancellationToken)
        {
            return (IQueryable<TInterface>)DbContext.Queryable<TEntity>();
        }
    }
}
