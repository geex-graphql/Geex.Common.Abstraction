using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

using StackExchange.Redis.Extensions.Core.Configuration;

namespace Geex.Common.Abstraction
{
    public class GeexCoreModuleOptions : IGeexModuleOption<GeexCoreModule>
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/geex";
        /// <summary>
        /// 后端host基地址
        /// </summary>
        public string Host { get; set; } = "http://localhost:8000";
        public string AppName { get; set; } = "geex";
        public RedisConfiguration? Redis { get; set; }
        /// <summary>
        /// 是否在response中抛出异常信息
        /// </summary>
        public bool IncludeExceptionDetails { get; set; } = true;
        /// <summary>
        /// 是否使用migration自动初始化数据
        /// </summary>
        public bool AutoMigration { get; set; } = false;
        /// <summary>
        /// 分页获取数据最多数量
        /// </summary>
        public int? MaxPageSize { get; set; } = 1000;
    }
}
