using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Geex.Common.Abstraction;
using Geex.Common.Abstractions;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using MongoDB.Entities;

using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.System.Text.Json;

// ReSharper disable once CheckNamespace
namespace StackExchange.Redis.Extensions.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Add StackExchange.Redis with its serialization provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="redisConfiguration">The redis configration.</param>
        /// <typeparam name="T">The typof of serializer. <see cref="ISerializer" />.</typeparam>
        public static IServiceCollection AddStackExchangeRedisExtensions(this IServiceCollection services)
        {
            var redisConfiguration = services.GetSingletonInstance<GeexCoreModuleOptions>();
            services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            services.AddSingleton<IRedisDatabase>(x => x.GetService<IRedisCacheClient>()?.Db0!);
            services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<ISerializer, SystemTextJsonSerializer>(x => new SystemTextJsonSerializer(Json.DefaultSerializeSettings));

            services.AddSingleton((provider) =>
            {
                return provider.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration();
            });

            services.AddSingleton(redisConfiguration.Redis);

            return services;
        }

        public static string GetUniqueId(this object obj)
        {
            if (obj is IEntity entity)
            {
                return entity.Id;
            }

            if (obj is IValueObject valueObject)
            {
                return string.Join("", valueObject.EqualityComponents.Select(x => x.GetUniqueId()));
            }
            var idProp = obj.GetType().GetProperty("Id");
            if (idProp != default)
            {
                return idProp.GetValue(obj)?.ToString() ?? "";
            }

            return obj.GetHashCode().ToString();
        }

        public static async Task<T> GetNamedAsync<T>(this IRedisDatabase service, string key)
        {
            return (await service.GetAsync<T>($"{typeof(T).Name}:{key}"));
        }

        public static async Task<IDictionary<string, T>> GetAllNamedByKeyAsync<T>(this IRedisDatabase service, string searchPattern = default)
        {
            var keys = await service.SearchKeysAsync($"{typeof(T).Name}:{searchPattern ?? "*"}");
            var result = (await service.GetAllAsync<T>(keys));
            return result;
        }

        public static async Task<bool> RemoveNamedAsync<T>(this IRedisDatabase service, string key)
        {
            return await service.RemoveAsync($"{typeof(T).Name}:{key}");
        }

        public static async Task<bool> RemoveAllNamedAsync<T>(this IRedisDatabase service)
        {
            return await service.RemoveAsync($"{typeof(T).Name}");
        }

        public static async Task<T> GetAndRemoveAsync<T>(this IRedisDatabase service, T obj)
        {
            var result = await service.GetNamedAsync<T>(obj.GetUniqueId());
            await service.RemoveNamedAsync<T>(obj.GetUniqueId());
            return result;
        }

        public static async Task<bool> SetNamedAsync<T>(
            this IRedisDatabase service,
          T obj,
          TimeSpan? expireIn = default,
          CancellationToken token = default(CancellationToken)) where T : class
        {
            if (expireIn.HasValue)
            {
                return await service.AddAsync<T>($"{typeof(T).Name}:{obj.GetUniqueId()}", obj, expireIn.Value);
            }
            return await service.AddAsync<T>($"{typeof(T).Name}:{obj.GetUniqueId()}", obj);
        }
    }
}
