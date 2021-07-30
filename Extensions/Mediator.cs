﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

using MediatR;

using MoreLinq;

// ReSharper disable once CheckNamespace
namespace Mediator
{
    public static class MediatorExtensions
    {
        static ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> mapDictionary = new();
        public static void SetEntity<T, TEntity>(this T value, TEntity target, params string[] ignoredPropNames) where T : IBaseRequest where TEntity : Entity
        {
            var cachedSrcMap = mapDictionary.GetOrAdd(typeof(T), x => x.GetProperties(BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public).ToDictionary(y => y.Name, y => y));
            var cachedTargetMap = mapDictionary.GetOrAdd(typeof(TEntity), x => x.GetProperties(BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public).ToDictionary(y => y.Name, y => y));
            var overlaps = cachedSrcMap.WhereIf(ignoredPropNames.Any(), x => !ignoredPropNames.Contains(x.Key)).Join(cachedTargetMap, l => l.Key, r => r.Key, (srcProp, targetProp) => (srcProp, targetProp));
            foreach (var ((_, srcProp), (_, targetProp)) in overlaps)
            {
                targetProp.SetValue(target, srcProp.GetValue(value));
            }
        }
    }
}