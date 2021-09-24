using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class SystemExtensions
    {
        /// <summary>
        /// 用于强制触发属性getter调用
        /// </summary>
        /// <param name="value"></param>
        public static object Call(this object value)
        {
            return value;
        }
    }
}
