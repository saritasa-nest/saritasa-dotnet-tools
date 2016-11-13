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
