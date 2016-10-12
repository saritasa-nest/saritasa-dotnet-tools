############
Repositories
############

********
Overview
********

You can store message to repository (database, files, any other destination) for logging purposes. Since the message is stored in structure way you can use it afterwards:

- to analyze what actions has been done by user(-s) or against any application component;
- to get most popular/long query;
- to repeat command/query/event;
- to apply structure queries (if MongoDB or Elasticsearch is used) since usualy it is stored as JSON;
- to get exception details if something wrong has happaned to system;

For example when user updates product details following JSON will be stored to configured data storage:

    .. code-block:: json

        {
            "HandlerType": "SandBox.CommandsHandlers, SandBox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            "ErrorMessage": "Test",
            "ErrorType": "System.Exception",
            "ContentType": "SandBox.Commands.UpdateProductCommand",
            "Id": "ff369938-1e78-4dae-80dc-c22ccceef724",
            "Type": 1,
            "Content": {
                "ProductId": 10,
                "Name": "Test",
                "BestBefore": null
            },
            "Data": {},
            "Error": {
                "ClassName": "System.Exception",
                "Message": "Test",
                "Data": null,
                "InnerException": null,
                "HelpURL": null,
                "StackTraceString": "   at SandBox.CommandsHandlers.HandleProductUpdate(UpdateProductCommand command, IProductsRepository productsRepository) in D:\\work2\\saritasatools\\samples\\SandBox\\SandBox\\CommandsHandlers.cs:line 26\r\n--- End of stack trace from previous location where exception was thrown ---\r\n   at Saritasa.Tools.Commands.CommandPipeline.Handle(Object command) in D:\\work2\\saritasatools\\src\\Saritasa.Tools\\Commands\\CommandPipeline.cs:line 27\r\n   at SandBox.Program.Test() in D:\\work2\\saritasatools\\samples\\SandBox\\SandBox\\Program.cs:line 86",
                "RemoteStackTraceString": null,
                "RemoteStackIndex": 0,
                "ExceptionMethod": "8\nHandleProductUpdate\nSandBox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\nSandBox.CommandsHandlers\nVoid HandleProductUpdate(SandBox.Commands.UpdateProductCommand, SandBox.IProductsRepository)",
                "HResult": -2146233088,
                "Source": "SandBox",
            },
            "CreatedAt": "2016-10-11T21:12:31.4461153+07:00",
            "ExecutionDuration": 14,
            "Status": 2
        }

You can see that it contains command content, error details (included stacktrace) and execution duration.

To use it you should call ``AppendMiddleware`` on your message pipeline and pass ``RepositoryMiddleware`` instance. The class requires ``IMessageRepository`` object. Here is an example:

    .. code-block:: c#

        var inMemoryMessageRepository = new InMemoryMessageRepository();
        QueryPipeline.AppendMiddlewares(new RepositoryMiddleware(inMemoryMessageRepository));  

You can also set ``RepositoryMessagesFilter`` to filter what messages to store.

************
Repositories
************

There are built in repositories.

AdoNetMessageRepository
=======================

CsvFileMessageRepository
========================

FileMessageRepository
=====================

InMemoryMessageRepository
=========================

NullMessageRepository
=====================
