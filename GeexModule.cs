using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Geex.Common.Abstraction;

using HotChocolate.Execution.Configuration;

using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoDB.Bson.Serialization;
using MongoDB.Entities;

using Volo.Abp;
using Volo.Abp.Modularity;

namespace Geex.Common.Abstractions
{
    public abstract class GeexModule<T> : GeexModule where T : GeexModule
    {
        public IConfiguration Configuration { get; private set; }

        public virtual void ConfigureModuleOptions(Action<IGeexModuleOption<T>> optionsAction)
        {
            var type = this.GetType().Assembly.ExportedTypes.FirstOrDefault(x => x.IsAssignableTo<IGeexModuleOption<T>>());
            if (type == default)
            {
                throw new InvalidOperationException($"{nameof(IGeexModuleOption<T>)} of {nameof(T)} is not declared, cannot be configured.");
            }
            var options = (IGeexModuleOption<T>?)this.ServiceConfigurationContext.Services.GetSingletonInstanceOrNull(type);
            optionsAction.Invoke(options!);
            this.ServiceConfigurationContext.Services.TryAdd(new ServiceDescriptor(type, options));
        }
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            Configuration = context.Services.GetConfiguration();
            context.Services.Add(new ServiceDescriptor(typeof(GeexModule), this));
            context.Services.Add(new ServiceDescriptor(this.GetType(), this));
            this.InitModuleOptions();
            base.PreConfigureServices(context);
        }

        private void InitModuleOptions()
        {
            var type = this.GetType().Assembly.ExportedTypes.FirstOrDefault(x => x.IsAssignableTo<IGeexModuleOption<T>>());
            if (type == default)
            {
                return;
            }
            var options = Activator.CreateInstance(type) as IGeexModuleOption<T>;
            Configuration.GetSection(type.Name).Bind(options);
            this.ServiceConfigurationContext.Services.TryAdd(new ServiceDescriptor(type, options));
        }

        public virtual void ConfigureModuleEntityMaps()
        {
            var entityConfigs = this.GetType().Assembly.ExportedTypes.Where(x => x.IsAssignableTo<IEntityMapConfig>() && !x.IsAbstract);
            foreach (var entityMapConfig in entityConfigs)
            {
                var entityType = entityMapConfig.BaseType!.GetGenericArguments().First();
                var instance = Activator.CreateInstance(entityMapConfig);
                var method = entityMapConfig.GetMethods().First(x => x.Name == nameof(EntityMapConfig<IEntity>.Map));
                var bsonClassMapInstance = Activator.CreateInstance(typeof(BsonClassMap<>).MakeGenericType(entityType));
                method.Invoke(instance, new[] { bsonClassMapInstance });
                BsonClassMap.RegisterClassMap(bsonClassMapInstance as BsonClassMap);
            }
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            this.ConfigureModuleEntityMaps();
            context.Services.AddMediatR(typeof(T));
            base.ConfigureServices(context);
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            foreach (var hasStartupInitialize in context.ServiceProvider.GetServices<IStartupInitializer>())
            {
                hasStartupInitialize.Initialize().Wait();
            }
            base.OnApplicationInitialization(context);
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            base.OnApplicationShutdown(context);
        }

        public override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            base.OnPostApplicationInitialization(context);
        }

        public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
        {
            base.OnPreApplicationInitialization(context);
        }

        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            base.PostConfigureServices(context);
        }

    }
    public class GeexModule : AbpModule
    {
        public static HashSet<Assembly> KnownModuleAssembly { get; } = new HashSet<Assembly>();
    }

    public abstract class GeexEntryModule<T> : GeexModule<T> where T : GeexModule
    {
        public IRequestExecutorBuilder SchemaBuilder => this.ServiceConfigurationContext.Services.GetSingletonInstance<IRequestExecutorBuilder>();
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.GetSingletonInstance<IRequestExecutorBuilder>().AddModuleTypes(this.GetType());
            base.ConfigureServices(context);
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            DB.MigrateAsync<T>().Wait();
            base.OnApplicationInitialization(context);
        }
    }
}
