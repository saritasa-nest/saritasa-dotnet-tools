Configuration
=============

Configuration Manager
---------------------

The middlewares can be configured using configuration file (``App.config`` or ``Web.config``). Example:

    .. code-block:: xml

        <configSections>
            <section name="pipelines" type="Saritasa.Tools.Messages.Configuration.XmlConfigSectionHandler, Saritasa.Tools.Messages"/>
        </configSections>

        <pipelines>
            <pipeline type="Saritasa.Tools.Messages.Commands.CommandPipeline">
            <middleware
                id="Locator"
                type="Saritasa.Tools.Messages.Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware, Saritasa.Tools.Messages"
                assemblies="Saritasa.Tools.Messages.StressTester.exe;Saritasa.Tools.Messages.StressTester.exe"/>
            <middleware
                id="Resolver"
                type="Saritasa.Tools.Messages.Commands.PipelineMiddlewares.CommandHandlerResolverMiddleware, Saritasa.Tools.Messages"
                />
            <middleware
                id="Executor"
                type="Saritasa.Tools.Messages.Commands.PipelineMiddlewares.CommandHandlerExecutorMiddleware, Saritasa.Tools.Messages"/>
            <middleware
                id="Json"
                type="Saritasa.Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware, Saritasa.Tools.Messages"
                active="False"
                repositorytype="Saritasa.Tools.Messages.Common.Repositories.JsonFileMessageRepository, Saritasa.Tools.Messages"
                path="D:\temp\logs"
                prefix="tmp"
                buffer="True" />
            <middleware
                id="Csv"
                type="Saritasa.Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware, Saritasa.Tools.Messages"
                active="False"
                repositorytype="Saritasa.Tools.Messages.Common.Repositories.CsvFileMessageRepository, Saritasa.Tools.Messages"
                path="D:\temp\logs"
                writeheader="True"
                delimeter=","
                prefix="tmp"
                buffer="True" />
            <middleware
                id="File"
                type="Saritasa.Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware, Saritasa.Tools.Messages"
                active="False"
                repositorytype="Saritasa.Tools.Messages.Common.Repositories.FileMessageRepository, Saritasa.Tools.Messages"
                path="D:\temp\logs"
                prefix="tmp"
                compress="False"
                buffer="True" />
            <middleware
                id="AdoNet"
                type="Saritasa.Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware, Saritasa.Tools.Messages"
                active="True"
                repositorytype="Saritasa.Tools.Messages.Common.Repositories.AdoNetMessageRepository, Saritasa.Tools.Messages"
                dialect="SqlServer"
                keepconnection="False"
                factory="System.Data.SqlClient"
                connectionstring="data source=.;initial catalog=Test;user id=sa;password=123;multipleactiveresultsets=True;"
                />
            <middleware
                id="WebService"
                type="Saritasa.Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware, Saritasa.Tools.Messages"
                active="False"
                rethrowexceptions="False"
                repositorytype="Saritasa.Tools.Messages.Common.Repositories.WebServiceRepository, Saritasa.Tools.Messages"
                uri="http://misc.anti-soft.ru/reqwriter/test.txt"
                buffer="True" />
            </pipeline>
        </pipelines>

To load use following code:

    .. code-block:: c#

        pipelineService.PipelineContainer = Saritasa.Tools.Messages.Configuration.XmlConfiguration.AppConfig;

Code Configuration
------------------

Take a look at specified pipeline section to see how to setup specific pipeline.
