using System;

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
