using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Volo.Abp.DependencyInjection;

namespace Geex.Common.Abstractions
{
    public class LazyFactory<T> : IScopedDependency
    {
        private readonly IServiceProvider _provider;
        public T? Value =>_provider.GetService<T>();
        public LazyFactory(IServiceProvider provider)
        {
            _provider = provider;
        }
    }
}
