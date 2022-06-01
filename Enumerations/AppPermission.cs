using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

using Humanizer;

namespace Geex.Common.Authorization
{
    public class AppPermission : Enumeration<AppPermission>
    {
        public AppPermission(string value) : base(value)
        {

        }
    }

    public abstract class AppPermission<TImplementation> : AppPermission
    {
        public AppPermission(PermissionString value) : base(value)
        {

        }
    }
}
