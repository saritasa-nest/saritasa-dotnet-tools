Messages
========

The main idea is to process application business logic in general way.

.. toctree::
    :titlesonly:

    commands
    queries
    events
    repositories
    typesresolve

You can setup you application to process everything thru single pipeline:

    ::

        Commands ---> Handle ---> (message) ---> Commands Pipeline ---> (processing) ---> Action
        Queries ---> Execute ---> (message) ---> Queries Pipeline ---> (processing) ---> Result
        Events ---> Raise ---> (message) ---> Events Pipeline ---> (processing) ---> Actions

Pipeline is a set of handlers (middlewares) to process your commands/queries. Here is a sample pipeline:

    ::

        CommandValidation ---> CommandHandlerLocator ---> CommandExecutor ---> Repository

Middleware makes an action on your message. You can create and insert your own custom middleware into pipeline to add additional logic to whole application.

Message
-------

Every request that goes thru pipeline is converted to ``Message`` instance. Properies:

==================== ==============================================================================================
Id                   Unique message id. Can be used to identify messages across systems.
Type                 Byte field to identify pipeline type. Right now only Commands, Queries and Events.
ContentType          Content type. Usually it refers to .NET full qualify name.
Content              Serialized message content.
Data                 Dictionary with additional message data.
Error                Contains exception if occurs while processing. Serialized. Available in Failed state.
ErrorMessage         Human readable error text message.
ErrorType            Error type. Usually it refers to .NET full qualify name.
CreatedAt            When message has been created. Local time.
ExecutionDuration    Milliseconds of processing time.
Status               Status of execution. NotInitialized, Processing, Completed, Failed, Rejected.
==================== ==============================================================================================

Middlewares
-----------

There are general middlewares that can be used in pipeline.

    .. class:: DataMiddleware

        Requires action to update ``Message.Data`` dictionary. Id is ``Data``.

    .. class:: PerformanceCounterMiddleware

        Implements following performance counter metrics:

            - Total Messages Processed;
            - Messages per Second Processed;
            - Average Message Processing Duration;

        Default id is ``PerformanceCounter``.

    .. class:: RepositoryMiddleware

        Stores message to repository. See repository section for more details.

Object Serializers
------------------

To store message content and error we need to serialize it to string. Not all middlerwares and repositories support all object serializers. You can create your own serializer by implementing ``IObjectSerializer``. There are following serializers built-in:

- ``JsonObjectSerializer`` - Uses ``Newtonsoft.Json`` to convert to JSON string.
- ``XmlObjectSerializer`` - Convert to xml string. Is not supported for .NET Core.
- ``BinaryObjectSerializer`` - Convert to bytes array. Is not supported for .NET Core.

Execution Context
-----------------

Sometimes you need to access to message or message pipeline that process it. Use ``MessageExecutionContext.Current`` for that. Make sure it is initialized by checking ``MessageExecutionContext.IsInitialized`` property.

Frameworks
----------

* .NET 4.5.2
* .NET 4.6.1
* .NET Core 1.1
* .NET Standard 1.6
