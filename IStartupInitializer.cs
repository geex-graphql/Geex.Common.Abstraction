using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Geex.Common.Abstractions
{
    public interface IStartupInitializer: ITransientDependency
    {
        Task Initialize();
    }
}
