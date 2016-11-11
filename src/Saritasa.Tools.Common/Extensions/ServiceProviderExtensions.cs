using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class ServiceProviderExtensions
    {
        public static TService GetService<TService>(this IServiceProvider @this)
        {
            return (TService)@this.GetService(typeof(TService));
        }
    }
}
