using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

using Humanizer;

namespace Geex.Common.Authorization
{
    public class AppPermission : Enumeration<AppPermission, string>
    {
        public AppPermission(string value) : base(value)
        {

        }
    }

    public abstract class AppPermission<TImplementation> : AppPermission
    {
        private static string _moduleName = typeof(TImplementation).Name.RemovePostFix("Permission").Camelize();

        public AppPermission(string value) : base(_moduleName + "_" + value)
        {

        }
    }
}
