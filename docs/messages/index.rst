Messages
========

The main idea is to process application business logic in general way.

.. toctree::
    :titlesonly:

    commands
    queries
    events
    repositories
    di

You can setup your application to process everything thru single pipelines:

    ::

        Commands ---> Handle ---> (message) ---> Commands Pipeline ---> (processing) ---> Action
        Queries ---> Execute ---> (message) ---> Queries Pipeline ---> (processing) ---> Result
        Events ---> Raise ---> (message) ---> Events Pipeline ---> (processing) ---> Actions

Pipeline is a set of middlewares to process your commands/queries. Here is a sample pipeline:

    ::

        CommandValidation ---> CommandHandlerLocator ---> CommandExecutor ---> Repository

Middleware makes an action on your message context. You can create and insert your own custom middleware into pipeline to add additional logic to whole application. Here is a brief diagram:

    .. image:: messages-overview.png

    *mc - message context (IMessageContext interface)*

Overview
--------

There are certain goals we want to achieve with pipelines:

- Provide ``ServiceProvider`` instance with correct scope.
- Keep middlewares as singletons.
- Provide execution environemnt for every request. It should be thread safe.

In general you should work with ``IMessagePipelineService`` class. It has two main properties:

==================== ==============================================================================================
ServiceProvider      Current scoped service provider. It will be set to message scope later. Shoule be per request.
PipelineContainer    Set of pipelines related to application. Should be singleton.
==================== ==============================================================================================

When you use your DI container make ``IMessagePipelineService`` transient or per request and ``IMessagePipelineContainer`` instance as singleton. For ``IMessagePipelineContainer`` instance you can use ``DefaultMessagePipelineContainer`` class that provides array for pipelines. For ``IMessagePipelineService`` you can use built-in ``DefaultMessagePipelineService`` class.

In general to setup and use you should:

1. Create ``IMessagePipelineContainer`` instance, for example ``pipelineContainer``.
2. Register pipelines to ``pipelineContainer``.
3. Create ``IMessagePipelineService`` instance and assign ``pipelineContainer`` to it.
4. Call any of extension methods, for example ``HandleCommand`` or ``Query``.

Simple example:

    .. code-block:: c#

        // Setup.
        var pipelineService = new DefaultMessagePipelineService();
        pipelineService.PipelineContainer
            .AddCommandPipeline()
                .UseDefaultMiddlewares(Assembly.GetAssembly(typeof(Entities.User)));

        // Use.
        pipelineService.HandleCommand(new Commands.CreateUser
        {
            FirstName = "Test"
        });

Message
-------

Every request that goes thru pipeline is converted to ``MessageContext`` instance. Properties:

==================== ==============================================================================================
Id                   Unique message id. Can be used to identify messages across systems.
ServiceProvider      Current scoped service provider.
ContentId            Content identifier, usually represents content type.
Content              Content object to process.
Status               Status of execution. NotInitialized, Processing, Completed, Failed, Rejected.
FailException        Exception that occures during execution.
Pipeline             Current pipeline used in execution.
Items                Dictionary of custom items that are shared across middlewares and request.
==================== ==============================================================================================

Status mapping table of ``MessageContext.Status``:

==================== == ===========================================================================================
NotInitialized       0  Message just created and no middlewares were invoked yet. Initial state.
Processing           1  Message is in processing by middlewares state.
Completed            2  Message has been processed successfully.
Failed               3  Message has not been processed successfully. Look at ``MessageContext.FailException``.
Rejected             4  Message is not going to be processed. For example it was filtered or no handler.
==================== == ===========================================================================================

Middlewares
-----------

There are general middlewares that can be used in pipeline.

    .. class:: DataMiddleware

        Requires action to update ``Message.Data`` dictionary. Default id is ``DataMiddleware``.

    .. class:: PerformanceCounterMiddleware

        Implements following performance counter metrics:

            - Total Messages Processed;
            - Messages per Second Processed;
            - Average Message Processing Duration;

        Default id is ``PerformanceCounterMiddleware``.

    .. class:: RepositoryMiddleware

        Stores message to repository. See repository section for more details.

Object Serializers
------------------

To store message content and error we need to serialize it to string. Not all middlerwares and repositories support all object serializers. You can create your own serializer by implementing ``IObjectSerializer``. There are following serializers built-in:

- ``JsonObjectSerializer`` - Uses ``Newtonsoft.Json`` to convert to JSON string.
- ``XmlObjectSerializer`` - Convert to xml string. Not supported for .NET Core.
- ``BinaryObjectSerializer`` - Convert to bytes array. Not supported for .NET Core.

Frameworks
----------

* .NET 4.5.2
* .NET Standard 1.5
