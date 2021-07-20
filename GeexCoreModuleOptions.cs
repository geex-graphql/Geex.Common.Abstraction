using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

namespace Geex.Common.Abstraction
{
    public class GeexCoreModuleOptions : IGeexModuleOption<GeexCoreModule>
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017/geex";
        public string AppName { get; set; } = "geex";
    }
}
