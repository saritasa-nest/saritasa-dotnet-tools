using System;
using Autofac;
using Saritasa.Tools.Messages.Abstractions;

namespace ZergRushCo.Todosya.Infrastructure
{
    /// <summary>
    /// Autofac implementation for <see cref="IServiceProviderFactory" />.
    /// </summary>
    public class AutofacServiceProviderFactory : IServiceProviderFactory
    {
        private readonly IContainer container;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="container">Autofac container.</param>
        public AutofacServiceProviderFactory(IContainer container)
        {
            this.container = container;
        }

        /// <inheritdoc />
        public IServiceProvider Create()
        {
            var scope = container.BeginLifetimeScope();
            return new AutofacServiceProvider(scope);
        }
    }
}
