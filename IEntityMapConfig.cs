using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson.Serialization;
using MongoDB.Entities;

namespace Geex.Common.Abstraction
{
    public abstract class EntityMapConfig<TEntity> : IEntityMapConfig where TEntity : IEntity
    {
        public abstract void Map(BsonClassMap<TEntity> map);
    }

    public interface IEntityMapConfig
    {

    }
}
