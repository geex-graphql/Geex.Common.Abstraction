using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Autofac;

using Geex.Common;
using Geex.Common.Abstraction;
using Geex.Common.Abstraction.Auditing;
using Geex.Common.Abstraction.Gql;
using Geex.Common.Abstraction.Storage;
using Geex.Common.Abstractions;
using Geex.Common.Gql;
using Geex.Common.Gql.Roots;
using Geex.Common.Gql.Types;

using HotChocolate.AspNetCore;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.AspNetCore.Voyager;
using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution.Options;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Pagination;
using HotChocolate.Utilities;

using ImpromptuInterface;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Linq;
using MongoDB.Entities;

using Volo.Abp.Modularity;

using Entity = Geex.Common.Abstraction.Storage.Entity;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class Extension
    {
        public static IServiceCollection AddStorage(this IServiceCollection builder)
        {
            var commonModuleOptions = builder.GetSingletonInstance<GeexCoreModuleOptions>();
            var mongoUrl = new MongoUrl(commonModuleOptions.ConnectionString) { };
            var mongoSettings = MongoClientSettings.FromUrl(mongoUrl);
            //mongoSettings.LinqProvider = LinqProvider.V3;
            mongoSettings.ClusterConfigurator = cb =>
            {
                if (commonModuleOptions.EnableDataLogging)
                {
                    ILogger<IMongoDatabase> logger = default;
                    cb.Subscribe<CommandStartedEvent>(e =>
                    {
                        logger ??= builder.GetServiceProviderOrNull()?.GetService<ILogger<IMongoDatabase>>();
                        logger?.LogInformation($"MongoDbCommandStartedEvent: {e.CommandName} - {e.Command.ToJson()}");
                    });
                    cb.Subscribe<CommandSucceededEvent>(e =>
                    {
                        logger?.LogInformation($"MongoDbCommandStartedEvent: {e.CommandName} success in {e.Duration.TotalMilliseconds}ms. Result: {e.Reply.ToJson()}");
                    });
                }
            };
            mongoSettings.ApplicationName = commonModuleOptions.AppName;
            DB.InitAsync(mongoUrl.DatabaseName ?? commonModuleOptions.AppName, mongoSettings).Wait();
            //builder.AddScoped(x => new DbContext(transactional: true));
            builder.AddScoped<IUnitOfWork>(x =>
            {
                if (x.TryGetService(typeof(IHttpContextAccessor), out var acce) && (acce as IHttpContextAccessor)?.HttpContext?.Request.Headers.TryGetValue("gqlType", out var gqlType) == true && gqlType == "query")
                {
                    return new WrapperUnitOfWork(new GeexDbContext(x, entityTrackingEnabled: false), x.GetService<ILogger<IUnitOfWork>>());
                }
                return new WrapperUnitOfWork(new GeexDbContext(x, transactional: true, entityTrackingEnabled: true), x.GetService<ILogger<IUnitOfWork>>());
            });
            // 直接从当前uow提取
            builder.AddScoped<DbContext>(x => (x.GetService<IUnitOfWork>() as WrapperUnitOfWork)!.DbContext);
            return builder;
        }

        public static IServiceCollection AddHttpResultSerializer<T>(
      this IServiceCollection services, Func<IServiceProvider, T> instance)
      where T : class, IHttpResultSerializer
        {
            services.RemoveAll<IHttpResultSerializer>();
            services.AddSingleton<IHttpResultSerializer, T>(instance);
            return services;
        }
        public static IRequestExecutorBuilder AddModuleTypes<TModule>(this IRequestExecutorBuilder schemaBuilder)

        {
            return schemaBuilder
                .AddModuleTypes(typeof(TModule));
        }
        public static object? GetSingletonInstanceOrNull(this IServiceCollection services, Type type) => services.FirstOrDefault<ServiceDescriptor>((Func<ServiceDescriptor, bool>)(d => d.ServiceType == type))?.ImplementationInstance;
        public static IRequestExecutorBuilder AddModuleTypes(this IRequestExecutorBuilder schemaBuilder, Type gqlModuleType)
        {
            if (GeexModule.KnownModuleAssembly.AddIfNotContains(gqlModuleType.Assembly))
            {
                var dependedModuleTypes = gqlModuleType.GetCustomAttribute<DependsOnAttribute>()?.DependedTypes;
                if (dependedModuleTypes?.Any() == true)
                {
                    foreach (var dependedModuleType in dependedModuleTypes)
                    {
                        schemaBuilder.AddModuleTypes(dependedModuleType);
                    }
                }
                var exportedTypes = gqlModuleType.Assembly.GetExportedTypes();

                var objectTypes = exportedTypes.Where(x => !x.IsAbstract && AbpTypeExtensions.IsAssignableTo<IType>(x)).Where(x => !x.IsGenericType || (x.IsGenericType && x.GenericTypeArguments.Any())).ToList();
                schemaBuilder.AddTypes(objectTypes.ToArray());

                var extensionTypes = exportedTypes.Where(x =>
                                                         (!x.IsAbstract && AbpTypeExtensions.IsAssignableTo<ObjectTypeExtension>(x))).ToArray();
                //schemaBuilder.AddTypes(rootTypes);
                foreach (var extensionType in extensionTypes)
                {
                    schemaBuilder.AddTypeExtension(extensionType);

                }

                var classEnumTypes = exportedTypes.Where(x => !x.IsAbstract && x.IsClassEnum() && x.Name != nameof(Enumeration)).ToList();
                foreach (var classEnumType in classEnumTypes)
                {
                    schemaBuilder.BindRuntimeType(classEnumType, typeof(EnumerationType<,>).MakeGenericType(classEnumType, classEnumType.GetClassEnumValueType()));
                    schemaBuilder.AddConvention<IFilterConvention>(new FilterConventionExtension(x =>
                    {
                        x.BindRuntimeType(classEnumType, typeof(ClassEnumOperationFilterInput<>).MakeGenericType(classEnumType));
                    }));
                }

                var directiveTypes = exportedTypes.Where(x => AbpTypeExtensions.IsAssignableTo<DirectiveType>(x)).ToList();
                foreach (var directiveType in directiveTypes)
                {
                    schemaBuilder.AddDirectiveType(directiveType);
                }

                foreach (var socketInterceptor in exportedTypes.Where(x => AbpTypeExtensions.IsAssignableTo<ISocketSessionInterceptor>(x)).ToList())
                {
                    schemaBuilder.ConfigureSchemaServices(s => s.TryAdd(ServiceDescriptor.Scoped(typeof(ISocketSessionInterceptor), socketInterceptor)));
                }

                foreach (var requestInterceptor in exportedTypes.Where(x => AbpTypeExtensions.IsAssignableTo<IHttpRequestInterceptor>(x)).ToList())
                {
                    schemaBuilder.ConfigureSchemaServices(s => s.TryAdd(ServiceDescriptor.Scoped(typeof(IHttpRequestInterceptor), requestInterceptor)));
                }
            }
            return schemaBuilder;
        }

        public static bool IsValidEmail(this string str)
        {
            return new Regex(@"\w[-\w.+]*@([A-Za-z0-9][-A-Za-z0-9]+\.)+[A-Za-z]{2,14}").IsMatch(str);
        }

        public static bool IsValidPhoneNumber(this string str)
        {
            return new Regex(@"\d{11}").IsMatch(str);
        }



        public static bool IsClassEnum(this Type type)
        {
            if (type.IsValueType)
            {
                return false;
            }

            return AbpTypeExtensions.IsAssignableTo<IEnumeration>(type);
        }

        public static IEnumerable<T> GetSingletonInstancesOrNull<T>(this IServiceCollection services) => services.Where<ServiceDescriptor>((Func<ServiceDescriptor, bool>)(d => d.ServiceType == typeof(T)))?.Select(x => x.ImplementationInstance).Cast<T>();

        public static IEnumerable<T> GetSingletonInstances<T>(this IServiceCollection services) => services.GetSingletonInstancesOrNull<T>() ?? throw new InvalidOperationException("Could not find singleton service: " + typeof(T).AssemblyQualifiedName);

        public static IServiceCollection ReplaceAll(
      this IServiceCollection collection,
      ServiceDescriptor descriptor)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));
            var serviceDescriptors = collection.Where((Func<ServiceDescriptor, bool>)(s => s.ServiceType == descriptor.ServiceType)).ToList();
            if (serviceDescriptors.Any())
                collection.RemoveAll(serviceDescriptors);
            collection.Add(descriptor);
            return collection;
        }
    }
}
