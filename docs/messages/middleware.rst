Middleware
==========

Custom Middleware
-----------------

You can develop your own custom middleware. This will allow you to add custom behavior for all messages handling. To do that your middleware class must implement ``IMessagePipelineMiddleware`` interface. It has ``Handle`` method and accepts message context. Also it has ``Id`` property, by default it should be class name. Here is an example:

    .. code-block:: c#

        /// <summary>
        /// Custom middleware.
        /// </summary>
        public class CustomMiddleware : IMessagePipelineMiddleware
        {
            /// <inheritdoc />
            public string Id => nameof(CustomMiddleware);

            /// <inheritdoc />
            public void Handle(IMessageContext messageContext)
            {
                System.Console.WriteLine(messageContext.ToString());
            }
        }

    Note that there is async version of middleware interface ``IAsyncMessagePipelineMiddleware``. Implement it as well to handle async messages.

Validate Commands
-----------------

You can add objects validation to command pipeline as first middleware. This would allow validate objects before use using data annotation attributes. Here is a sample middleware you can use.

    .. code-block:: c#

        /// <summary>
        /// Validates command. Uses data annotation attributes.
        /// </summary>
        public class CommandValidationMiddleware : IMessagePipelineMiddleware
        {
            /// <inheritdoc />
            public string Id { get; set; } = nameof(CommandValidationMiddleware);

            /// <summary>
            /// Throw exception if object is not valid.
            /// </summary>
            public bool ThrowException { get; set; } = true;

            /// <inheritdoc />
            public virtual void Handle(IMessageContext messageContext)
            {
                try
                {
                    Saritasa.Tools.Domain.Exceptions.ValidationException.ThrowFromObjectValidation(messageContext.Content);
                }
                catch (Exception)
                {
                    messageContext.Status = ProcessingStatus.Rejected;
                    if (ThrowException)
                    {
                        throw;
                    }
                }
            }
        }

Validate Queries
----------------

For query pipeline you can validate input method argments. Here is a middleware you can use:

    .. code-block:: c#

        /// <summary>
        /// Validates query input arguments. Uses data annotation attributes.
        /// </summary>
        public class QueryValidationMiddleware : IMessagePipelineMiddleware
        {
            /// <inheritdoc />
            public string Id { get; set; } = nameof(QueryValidationMiddleware);

            /// <summary>
            /// Throw exception if object is not valid.
            /// </summary>
            public bool ThrowException { get; set; } = true;

            /// <inheritdoc />
            public virtual void Handle(IMessageContext messageContext)
            {
                try
                {
                    // The dictionary contains named query parameters.
                    var dictionary = messageContext.Content as IDictionary<string, object>;

                    if (dictionary == null)
                    {
                        return;
                    }

                    foreach (var value in dictionary.Values)
                    {
                        if (value is object)
                        {
                            Saritasa.Tools.Domain.Exceptions.ValidationException.ThrowFromObjectValidation(value);
                        }
                    }
                }
                catch (Exception)
                {
                    messageContext.Status = ProcessingStatus.Rejected;
                    if (ThrowException)
                    {
                        throw;
                    }
                }
            }
        }

Domain Events
-------------

You can use domain events with events pipeline:

    .. code-block:: c#

        /// <summary>
        /// Uses domain events manager to raise events.
        /// </summary>
        public class DomainEventLocatorMiddleware : IMessagePipelineMiddleware
        {
            /// <inheritdoc />
            public string Id { get; set; } = nameof(DomainEventLocatorMiddleware);

            private readonly IDomainEventsManager eventsManager;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="parameters">Parameters dictionary.</param>
            public DomainEventLocatorMiddleware(IDictionary<string, string> parameters)
            {
                throw new NotSupportedException("The middleware does not support instantiation from parameters dictionary.");
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="eventsManager">Domain events manager.</param>
            public DomainEventLocatorMiddleware(IDomainEventsManager eventsManager)
            {
                if (eventsManager == null)
                {
                    throw new ArgumentNullException(nameof(eventsManager));
                }
                this.Id = GetType().Name;
                this.eventsManager = eventsManager;
            }

            /// <inheritdoc />
            public virtual void Handle(IMessageContext messageContext)
            {
                var hasHandlersGenericMethod = typeof(IDomainEventsManager).GetTypeInfo().GetMethod("HasHandlers")
                    .MakeGenericMethod(messageContext.Content.GetType());
                if ((bool)hasHandlersGenericMethod.Invoke(eventsManager, new object[] { }))
                {
                    var raiseGenericMethod = eventsManager.GetType().GetTypeInfo().GetMethod("Raise")
                        .MakeGenericMethod(messageContext.Content.GetType());

                    messageContext.Items.TryGetValue(EventHandlerLocatorMiddleware.HandlerMethodsKey, out object handlersObj);
                    var handlers = handlersObj as EventHandlerMethodWithObject[];
                    messageContext.Items[EventHandlerLocatorMiddleware.HandlerMethodsKey] =
                        AddItem(handlers, new EventHandlerMethodWithObject(raiseGenericMethod, eventsManager));
                }
            }

            private static T[] AddItem<T>(T[] arr, T item)
            {
                if (arr == null)
                {
                    return new[] { item };
                }
                var sourceLength = arr.Length;
                var newarr = new T[sourceLength + 1];
                Array.Copy(arr, newarr, sourceLength);
                newarr[sourceLength] = item;
                return newarr;
            }
        }

    