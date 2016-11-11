using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Internal
{
    public static class ServiceProviderExtension
    {
        public static TService GetService<TService>(this IServiceProvider @this)
        {
            return (TService)@this.GetService(typeof(TService));
        }
    }
}
