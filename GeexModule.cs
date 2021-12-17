using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Geex.Common.Abstraction;

using HotChocolate.Execution.Configuration;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using MongoDB.Bson.Serialization;
using MongoDB.Entities;

using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;

namespace Geex.Common.Abstractions
{
    public abstract class GeexModule<TModule, TModuleOptions> : GeexModule<TModule> where TModule : GeexModule where TModuleOptions : IGeexModuleOption<TModule>
    {
        protected new TModuleOptions ModuleOptions => this.ServiceConfigurationContext.Services.GetSingletonInstance<TModuleOptions>();
    }
    public abstract class GeexModule<TModule> : GeexModule where TModule : GeexModule
    {
        public IConfiguration Configuration { get; private set; }

        public virtual void ConfigureModuleOptions(Action<IGeexModuleOption<TModule>> optionsAction)
        {
            var type = this.GetType().Assembly.ExportedTypes.FirstOrDefault(x => x.IsAssignableTo<IGeexModuleOption<TModule>>());
            if (type == default)
            {
                throw new InvalidOperationException($"{nameof(IGeexModuleOption<TModule>)} of {nameof(TModule)} is not declared, cannot be configured.");
            }
            var options = (IGeexModuleOption<TModule>?)this.ServiceConfigurationContext.Services.GetSingletonInstanceOrNull(type);
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
            var type = this.GetType().Assembly.ExportedTypes.FirstOrDefault(x => x.IsAssignableTo<IGeexModuleOption<TModule>>());
            if (type == default)
            {
                return;
            }
            var options = Activator.CreateInstance(type) as IGeexModuleOption<TModule>;
            Configuration.GetSection(type.Name).Bind(options);
            this.ServiceConfigurationContext.Services.TryAdd(new ServiceDescriptor(type, options));
            this.ServiceConfigurationContext.Services.TryAdd(new ServiceDescriptor(typeof(IGeexModuleOption<TModule>), options));
        }

        protected virtual IGeexModuleOption<TModule> ModuleOptions => this.ServiceConfigurationContext.Services.GetSingletonInstance<IGeexModuleOption<TModule>>();

        public virtual void ConfigureModuleEntityMaps()
        {
            var entityConfigs = this.GetType().Assembly.ExportedTypes.Where(x => x.IsAssignableTo<IEntityMapConfig>() && !x.IsAbstract);
            foreach (var entityMapConfig in entityConfigs)
            {
                //一个map可能同时映射多个类型:public class ContractMapConfig : IEntityMapConfig<class1>, IEntityMapConfig<class2>,
                var interfaces = entityMapConfig.GetInterfaces().Where(x => x.IsAssignableTo<IEntityMapConfig>() && x.IsGenericType);
                var entityTypes = interfaces.Select(x => x.GetGenericArguments().First());
                var instance = Activator.CreateInstance(entityMapConfig);
                foreach (var entityType in entityTypes)
                {
                    var method = entityMapConfig.GetMethods().First(x => x.Name == nameof(IEntityMapConfig<IEntity>.Map) && x.GetParameters().First().ParameterType.GetGenericArguments()[0] == entityType);
                    var bsonClassMapType = typeof(BsonClassMap<>).MakeGenericType(entityType);
                    var bsonClassMapInstance = Activator.CreateInstance(bsonClassMapType);
                    method.Invoke(instance, new[] { bsonClassMapInstance });
                    if (!BsonClassMap.IsClassMapRegistered(entityType))
                    {
                        BsonClassMap.RegisterClassMap(bsonClassMapInstance as BsonClassMap);
                    }
                }
            }
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            this.ConfigureModuleEntityMaps();
            context.Services.AddMediatR(configuration: configuration =>
            {
            }, typeof(TModule));
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
            context.Services.GetSingletonInstanceOrNull<IRequestExecutorBuilder>()?.AddModuleTypes(this.GetType());
            base.PostConfigureServices(context);
        }

    }

    public class GeexModule : AbpModule
    {
        public IRequestExecutorBuilder SchemaBuilder => this.ServiceConfigurationContext.Services.GetSingletonInstance<IRequestExecutorBuilder>();
        public static HashSet<Assembly> KnownModuleAssembly { get; } = new HashSet<Assembly>();
    }

    public abstract class GeexEntryModule<T> : GeexModule<T> where T : GeexModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var env = context.Services.GetSingletonInstance<IWebHostEnvironment>();
            context.Services.AddCors(options =>
            {
                if (env.IsDevelopment())
                {
                    options.AddDefaultPolicy(x =>
                        x.SetIsOriginAllowed(x => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
                }
                else
                {
                    var corsRegex = Configuration.GetValue<string>("CorsRegex");
                    var regex = new Regex(corsRegex, RegexOptions.Compiled);
                    options.AddDefaultPolicy(x => x.SetIsOriginAllowed(origin => regex.Match(origin).Success).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
                }
            });
            base.ConfigureServices(context);
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var coreModuleOptions = context.ServiceProvider.GetService<GeexCoreModuleOptions>();
            if (coreModuleOptions.AutoMigration)
            {
                context.ServiceProvider.GetService<DbContext>().MigrateAsync<T>().Wait();
            }
            var app = context.GetApplicationBuilder();
            var _env = context.GetEnvironment();
            var _configuration = context.GetConfiguration();

            base.OnApplicationInitialization(context);
            app.UseGeexGraphQL();
        }
    }
}
