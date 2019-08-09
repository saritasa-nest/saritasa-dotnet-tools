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

        Store messages in CSV files. File name format is ``{prefix}-yyyyMMdd-XXX.csv`` (for example ``backend-20170101-001.csv``).

            .. attribute:: string logsPath

                Directory to store files.

            .. attribute:: IObjectSerializer serializer

                Serializer to be used to serialize message and error contents. JSON by default.

            .. attribute:: string prefix

                File name prefix.

            .. attribute:: bool buffer = true

                Should the output stream be buffered. The message will be stored to buffer before writing to disk.

    .. class:: JsonFileMessageRepository

        Store messages in JSON files. File name format is ``{prefix}-yyyyMMdd-XXX.json`` (for example ``backend-20170101-001.json``).

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

Query Repositories
------------------

Every repository supports simple query language, it has ``GetAsync`` method. To be able to construct query ``MessageQuery`` class is used. For example:

    .. code-block:: c#

        var repository = new Saritasa.Tools.Messages.Common.Repositories.AdoNetMessageRepository(
            DbProviderFactories.GetFactory("System.Data.SqlClient"),
            connectionString);
        var query = Saritasa.Tools.Messages.Common.MessageQuery.Create()
            .WithCreatedStartDate(new DateTime(2018, 10, 10))
            .WithStatus(ProcessingStatus.Failed)
            .WithRange(0, 500);
        var messages = repository.GetAsync(query).GetAwaiter().GetResult();

Also you can use string syntax to define query:

    .. code-block:: c#

        var repository = new Saritasa.Tools.Messages.Common.Repositories.AdoNetMessageRepository(
            DbProviderFactories.GetFactory("System.Data.SqlClient"),
            connectionString);
        var query = Saritasa.Tools.Messages.Common.MessageQuery.CreateFromString("created > 2018-10-10 status = failed take 500");
        var messages = repository.GetAsync(query).GetAwaiter().GetResult();

The query may have following tokens and operations. There is currently only AND operation supported between expressions. "Greater than" operation means "greater than and equal" as well as  "below than" mean "below than and equal".

============= ================== ==============================================================================================
Token         Operations         Description
------------- ------------------ ----------------------------------------------------------------------------------------------
id            ``=``              Identifier of message record. (ex ``id = "b6543f31-9c60-4be1-b2b0-69b8c3159c91"``)
created       ``>`` ``<`` ``=``  When message record has been created. (ex ``created > 2018-09-01 created < 2018-10-01``)
contenttype   ``=``              Message content (class) type starts with string.
                                 (ex ``ContentType = "MyApp.Domain.Commands.User"`` will find
                                 ``MyApp.Domain.Commands.UserCreate`` and ``MyApp.Domain.Commands.UserUpdate``).
errortype     ``=``              Error type (class) starts with string. (ex ``ErrorType = "MyApp.Exception"``)
status        ``=``              Message processing status, possible values are ``NotInitialized``, ``Processing``,
                                 ``Completed``, ``Failed``, ``Rejected``. (ex ``status = failed``)
type          ``=``              Message type. Values are 1 for command, 2 for query, 3 for event. (ex ``type = 1``,
                                 ``type = event``)
duration      ``>`` ``<`` ``=``  Execution duration limit in ms. (ex ``duration > 1000``)
skip          ``=``              Number of records to skip. (ex ``skip 10``)
take          ``=``              Number of records to take. (ex ``take 100``)
============= ================== ==============================================================================================

Examples:

    - ``created > 2018-10-10 status = failed take 500``
    - ``created > 2018-10-10 created < 2018-11-10 status = completed skip 100 take 500``
    - ``contenttype = MyApp.Domain.Commands.JiraSyncCommand duration > 1000``
