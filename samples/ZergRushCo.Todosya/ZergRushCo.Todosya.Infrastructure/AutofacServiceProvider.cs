using System;
using Autofac;

namespace ZergRushCo.Todosya.Infrastructure
{
    /// <summary>
    /// Autofac wrapper for <see cref="IServiceProvider" />.
    /// </summary>
    public class AutofacServiceProvider : IServiceProvider
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
    }
}
