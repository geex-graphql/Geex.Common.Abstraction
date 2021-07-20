using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core.Lifetime;

namespace Geex.Common.Abstraction
{
    public class ServiceLocator : IServiceProvider
    {
        public static ServiceLocator Current = new ServiceLocator();

        public object? GetService(Type serviceType)
        {
            return RootLifetimeScope.Resolve(serviceType);
        }

        public static void SetProvider(ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope.Tag != LifetimeScope.RootTag)
            {
                throw new InvalidOperationException("you should only set root provider for service locator.");
            }
            RootLifetimeScope = lifetimeScope;
        }

        public static ILifetimeScope RootLifetimeScope { get; private set; }
    }
}
