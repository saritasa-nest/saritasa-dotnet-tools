Dependency Injection
====================

Pipelines require service resolving to instaniate handlers and call handler methods. It is important to dispose them properly after use. By default resolver must dispose services after use. Handlers will be disposed automatically if they were created by pipeline middleware. Since ``IPipelineService`` is transient it may by injected as dependency to controller or service. Make sure you registered ``IServiceProvider`` property for pipeline service. That way all resolved services will be tracked and disposed by dependency injection (DI) container.

With your DI container make sure:

- you have registered ``IPipelineService`` as scoped, per web request, transient;
- you have registered ``IServiceProvider`` to properly dispose services (per web request, per lifetime scope);
- you have registered ``IMessagePipelineContainer`` as singleton;

There are some usages with different dependency injection containers.

No Dependency Injection
-----------------------

``DefaultMessagePipelineService`` has default parameterless constructor. So it can be used to set pipelines and service provider manually. If service provider is not set the ``NullServiceProvider`` will be used that returns ``null`` for every request. Here is an example:

    .. code-block:: c#

        var pipelineService = new DefaultMessagePipelineService();
        pipelineService.PipelineContainer = new DefaultMessagePipelineContainer
        {
            Pipelines = new IMessagePipeline[]
            {
                CommandPipeline,
                QueryPipeline,
                EventPipeline
            }
        };

Also you can use ``FuncServiceProvider`` class to assign delegate method for resolving.

    .. code-block:: c#

        public static object Resolver(Type type)
        {
            if (type == typeof(IProductsRepository))
            {
                return productsRepository;
            }
            return null;
        }

        pipelineService.ServiceProvider = new FuncServiceProvider(Resolver);

Autofac
-------

Here is an example of usage.

    .. code-block:: c#

        public DiConfig()
        {
            var pipelinesContainer = RegisterPipelines();
            builder.RegisterInstance(pipelinesContainer).As<IMessagePipelineContainer>().SingleInstance();
            builder.RegisterType<DefaultMessagePipelineService>().As<IMessagePipelineService>().InstancePerRequest()
                .InstancePerLifetimeScope();
            builder.RegisterType<AutofacServiceProvider>().As<IServiceProvider>().InstancePerRequest()
                .InstancePerLifetimeScope();
        }

        public static IMessagePipelineContainer RegisterPipelines()
        {
            var pipelinesContainer = new DefaultMessagePipelineContainer();

            // Empty, not configured pipelines.
            pipelinesContainer.AddCommandPipeline();
            pipelinesContainer.AddQueryPipeline();
            pipelinesContainer.AddEventPipeline();

            return pipelinesContainer;
        }

Special Autofac wrapper should be written to support ``IServiceProvider``.

    .. code-block:: c#

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

.NET Core
---------

Here is an example of configuration.

    .. code-block:: c#

        public DiConfig()
        {
            // Pipelines.
            var pipelineContainer = new DefaultMessagePipelineContainer();
            services.AddSingleton<IMessagePipelineContainer>(pipelineContainer);
            services.AddScoped<IMessagePipelineService, DefaultMessagePipelineService>();

            pipelineContainer.AddCommandPipeline()
                .UseDefaultMiddlewares(typeof(Domain.User.Entities.User).GetTypeInfo().Assembly);
            pipelineContainer.AddQueryPipeline()
                .UseDefaultMiddlewares();
            pipelineContainer.AddEventPipeline()
                .UseDefaultMiddlewares(typeof(Domain.User.Entities.User).GetTypeInfo().Assembly);
        }
