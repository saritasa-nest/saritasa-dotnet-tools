Repositories
============

Overview
--------

You can store message to repository (database, files, any other destination) for logging purposes. Since the message is stored in structure way you can use it afterwards:

- to analyze what actions has been done by user(-s) or against any application component;
- to get most popular/long query;
- to repeat command/query/event;
- to apply structure queries (if MongoDB or Elasticsearch is used) since usualy it is stored as JSON;
- to get exception details if something wrong has happaned to system;

For example when user updates product details following JSON will be stored to configured data storage:

    .. code-block:: json

        {
            "Id": "ff369938-1e78-4dae-80dc-c22ccceef724",
            "Type": 1,
            "ContentType": "SandBox.Commands.UpdateProductCommand",
            "Content": {
                "ProductId": 10,
                "Name": "Test",
                "BestBefore": null
            },
            "Data": {},
            "ErrorType": "System.Exception",
            "ErrorMessage": "Test",
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

To use it you should call ``AddMiddlewares`` on your message pipeline and pass ``RepositoryMiddleware`` instance. The class requires ``IMessageRepository`` object. Here is an example:

    .. code-block:: c#

        var inMemoryMessageRepository = new InMemoryMessageRepository();
        PipelineService.AddCommandPipline()
            .AppendMiddlewares(new RepositoryMiddleware(inMemoryMessageRepository));

You can also set ``RepositoryMessagesFilter`` to filter what messages to store. Repository middleware's name is the type name of repository type. For example if you use ``InMemoryMessageRepository`` the name of ``RepositoryMiddleware`` will be ``InMemoryMessageRepository`` as well.

Repositories
------------

There are built in repositories.

    .. class:: AdoNetMessageRepository

        Store messages in data source that supports ADO.NET provider. Following parameters are required:

            .. attribute:: DbProviderFactory factory

                Database factory class to be used. For example ``DbProviderFactories.GetFactory("MySql.Data.MySqlClient")``, ``DbProviderFactories.GetFactory("System.Data.SqlClient")``.

            .. attribute:: string connectionString

                Connection string. For example ``Server=127.0.0.1;Database=commands;Uid=root;Pwd=123;`` for MySQL or ``data source=.;initial catalog=Project.Development;user id=sa;password=123;``.

            .. attribute:: Dialect dialect

                SQL dialect. By default (Auto) will be determined from ``DbProviderFactory``. Following SQL providers are supoorted: ``SqlServer``, ``MySql``, ``Sqlite``.

            .. attribute:: IObjectSerializer serializer

                Serializer to be used to serialize message and error contents. JSON by default.

    .. class:: CsvFileMessageRepository

        Store messages in csv files. File name format is ``{prefix}-yyyyMMdd-XXX.csv`` (for example ``backend-20170101-001.csv``).

            .. attribute:: string logsPath

                Directory to store files.

            .. attribute:: IObjectSerializer serializer

                Serializer to be used to serialize message and error contents. JSON by default.

            .. attribute:: string prefix

                File name prefix.

            .. attribute:: bool buffer = true

                Should the output stream be buffered. The message will be stored to buffer before writing to disk.

    .. class:: FileMessageRepository

        Store messages into binary files. File name format is ``{prefix}-yyyyMMdd-XXX.csv`` (for example ``backend-20170101-001.bin``).

            .. attribute:: string logsPath

                Directory to store files.

            .. attribute:: IObjectSerializer serializer

                Serializer to be used to serialize message and error contents. JSON by default.

            .. attribute:: string prefix

                File name prefix.

            .. attribute:: bool buffer = true

                Should the output stream be buffered. The message will be stored to buffer before writing to disk.

            .. attribute:: bool compress = false

                Use GZip to compress files.

            .. note:: If compression is used files will have ``.zip`` extension.

    .. class:: InMemoryMessageRepository

        Store messages to plain in memory list. There is ``Dump`` method that returns all data in string.

    .. class:: NullMessageRepository

        Does nothing, for testing purposes.

    .. class:: ElasticsearchRepository

        Store messages to Elasticsearch. Default index name is ``saritasa.messages``.

            .. attribute:: uri

                Uri to Elasticsearch instance. For example default installed instance has ``http://localhost:9200`` address.

    .. class:: LogglyRepository

        Uses `Loggly <http://www.loggly.com>`_ service to send messages to.

            .. attribute:: token

                Customer token. Should be created within Loggly admin panel.
