Messages
========

The main idea is to process application business logic in general way.

Table of Contents
-----------------

.. toctree::
    :titlesonly:

    commands
    queries
    events

You can setup you application to process everything thru single pipeline:

    ::

        Commands---v
        Queries ---+---> (message) ---> Pipeline ---> (processing) ---> Result
        Events ----^

Pipeline is a set of handlers (middlewares) to process your commands/queries. Here is a sample pipeline:

    ::

        CommandValidation ---> CommandHandlerLocator ---> CommandExecutor ---> Repository

Middleware makes an action on your message. You can create and insert your own custom middleware into pipeline to add additional logic to whole application.
