using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson.Serialization;
using MongoDB.Entities;

using Volo.Abp.DependencyInjection;

namespace Geex.Common.Abstraction
{
    [Dependency(ServiceLifetime.Transient)]
    [ExposeServices(typeof(IEntityMapConfig))]
    public abstract class EntityMapConfig<TEntity> : IEntityMapConfig where TEntity : IEntity
    {
        public abstract void Map(BsonClassMap<TEntity> map);

        /// <inheritdoc />
        void IEntityMapConfig.Map()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(TEntity)))
            {
                var map = new BsonClassMap<TEntity>();
                this.Map(map);
                BsonClassMap.RegisterClassMap(map);
            }
        }
    }
    public interface IEntityMapConfig
    {
        public void Map();
    }
}
