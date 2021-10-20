using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Geex.Common.Abstractions;

namespace Geex.Common.Authorization
{
    public class AppPermission : Enumeration<AppPermission, string>
    {
        public AppPermission(string value) : base(value)
        {

        }


    }
}
