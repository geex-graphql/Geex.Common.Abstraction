using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class SystemExtensions
    {
        private static Dictionary<Type, MethodInfo> LazyGetterCache = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// 用于强制触发属性成员调用
        /// </summary>
        public static object? Call(this object value, MethodInfo method, Type[] typeArguments, object[] arguments = null)
        {
            return method.MakeGenericMethod(typeArguments).Invoke(value, arguments);
        }
        /// <summary>
        /// 反射获取lazy值(带缓存)
        /// </summary>
        public static object? Call(this object value, MethodInfo method, object[] arguments = null)
        {
            return method.Invoke(value, arguments);
        }

        /// <summary>
        /// 用于强制触发属性成员调用
        /// </summary>
        public static object? GetLazyValue(this object lazy, Type valueType)
        {

            if (LazyGetterCache.TryGetValue(valueType, out var method))
            {
                return method.Invoke(lazy, Array.Empty<object>());
            }
            var lazyType = lazy.GetType();
            method = lazyType.GetProperty(nameof(Lazy<object>.Value))!.GetMethod!;
            LazyGetterCache.Add(valueType, method);
            return method.Invoke(lazy, Array.Empty<object>());
        }
    }
}
