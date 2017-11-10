using System;
using Autofac;

namespace Saritasa.BoringWarehouse.Infrastructure
{
    /// <summary>
    /// Autofac wrapper for <see cref="IServiceProvider" />.
    /// </summary>
    public sealed class AutofacServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IComponentContext context;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="componentContext">The context in which a service can be accessed
        /// or a component's dependencies resolved. Disposal of a context will dispose any owned components.</param>
        public AutofacServiceProvider(IComponentContext componentContext)
        {
            this.context = componentContext;
        }

        /// <inheritdoc />
        public object GetService(Type serviceType) => context.Resolve(serviceType);

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            var disposable = context as IDisposable;
            disposable?.Dispose();
        }

        #endregion
    }
}
