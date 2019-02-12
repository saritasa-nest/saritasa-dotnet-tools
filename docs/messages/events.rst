Events
======

Overview
--------

Things happen. Not all of them are interesting, some may be worth recording but don't provoke a reaction. The most interesting ones cause a reaction. Many systems need to react to interesting events only. Events are reactions to something that happens to system. They are executed separately. For example after user creation welcome email should be sent to him. User creation is a command and email sending is an event.

    ::

        User Create ---> (events pipeline) ---> Email Send

Here is how it can be used:

1. Setup pipeline service. Also you need to register it with your DI container since it can be injected to events hanlders.

    .. code-block:: c#

        var pipelineContainer = new DefaultMessagePipelineContainer();
        pipelineContainer.AddEventPipeline()
            .AddStandardMiddlewares(options =>
            {
                options.SetAssemblies(typeof(Domain.Users.Entities.User).GetTypeInfo().Assembly);
                options.UseExceptionDispatchInfo = true;
            });

2. Create event class:

    .. code-block:: c#

        public class UserCreatedEvent
        {
            public User User { get; set; }
        }

3. Prepare event handler, it should be class with attribute ``EventHandlers``, the method name should start with ``Handle`` prefix and first argument should be event class.

    .. code-block:: c#

        [EventHandlers]
        public class UserEventsHandlers
        {
            public void HandleSendEmailOnUserCreate(UserCreatedEvent userCreatedEvent,
                Saritasa.Tools.Emails.IEmailSender<MailMessage> emailSender)
            {
                var message = new MailMessage
                {
                    To = new MailAddress[] { new MailAddress(userCreatedEvent.User.Email) },
                    Body = "Thanks for registering to our site!"
                };
                emailSender.SendAsync(message);
            }
        }

Middlewares
-----------

    .. class:: PrepareMessageContextMiddleware

        The middleware prepares message context for message processing. It fills ContentId field.

    .. class:: EventHandlerLocatorMiddleware

        Locates event hanlders. It stores methods within context items using ``handler-methods`` item key.

    .. class:: EventHandlerResolverMiddleware

        The middleware is to resolve handler object, create it and resolve all dependencies if needed. The resolved object is stored in ``handler-object`` item of context items.

        .. attribute:: UsePropertiesResolving

            Resolve handler object public properties using service provider. False by default.

    .. class:: EventHandlerExecutorMiddleware

        Included to default pipeline. Default event executor. It does not process events with Rejected status.

        .. attribute:: IncludeExecutionDuration

            Includes execution duration into processing result. The target item key is ``.execution-duration``. Default is true.

        .. attribute:: UseParametersResolve

            If true the middleware will try to resolve executing method parameters. False by default.

        .. attribute:: CaptureExceptionDispatchInfo

            Captures original exception and stack trace within handler method using ``System.Runtime.ExceptionServices.ExceptionDispatchInfo``. False by default.

Default Pipeline
----------------

    ::

        PrepareMessageContextMiddleware ---> EventHandlerLocatorMiddleware ---> EventHandlerResolverMiddleware ---> EventHandlerExecutorMiddleware
