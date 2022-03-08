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
        /// <summary>
        /// 用于强制触发属性成员调用
        /// </summary>
        public static object? Call(this object value, MethodInfo method, Type[] typeArguments, object[] arguments = null)
        {
            return method.MakeGenericMethod(typeArguments).Invoke(value, arguments);
        }
        /// <summary>
        /// 用于强制触发属性成员调用
        /// </summary>
        public static object? Call(this object value, MethodInfo method, object[] arguments = null)
        {
            return method.Invoke(value, arguments);
        }
    }
}
