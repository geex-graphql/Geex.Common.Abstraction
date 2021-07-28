using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediatR;

namespace Geex.Common.Abstraction.Auditing.Events
{
    public class EntitySubmittedNotification<TEntity> : INotification
    {
        public string EntityId { get; }

        public EntitySubmittedNotification(string entityId)
        {
            EntityId = entityId;
        }
    }
}
