using System;
using System.Configuration;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Emails.Interceptors;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Commands;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Events;
using Saritasa.Tools.Messages.Queries;
using ZergRushCo.Todosya.DataAccess;

namespace ZergRushCo.Todosya.Infrastructure
{
    /// <summary>
    /// Dependency injection configuration setup.
    /// </summary>
    public static class DiConfig
    {
        public static IContainer AutofacContainer { get; set; }

        public static object Resolve(Type type)
        {
            return AutofacContainer?.Resolve(type);
        }

        public static void Setup(ContainerBuilder builder, bool testingMode = false)
        {
            // Other bindings.
            var connectionStringConf = ConfigurationManager.ConnectionStrings["AppDbContext"];
            var connectionString = connectionStringConf.ConnectionString.Replace("{baseUrl}",
                AppDomain.CurrentDomain.BaseDirectory);
            builder.RegisterType<AppUnitOfWorkFactory>().AsSelf().AsImplementedInterfaces();
            builder.Register(c => c.Resolve<AppUnitOfWorkFactory>().Create()).AsImplementedInterfaces();
            builder.Register(c => new AppDbContext()).AsSelf();
            builder.Register<IUserStore<Domain.UserContext.Entities.User>>(
                    c => new UserStore<Domain.UserContext.Entities.User>(c.Resolve<AppDbContext>()))
                .AsImplementedInterfaces();
            builder.Register(
                    c => new Domain.UserContext.Services.AppUserManager(
                        c.Resolve<IUserStore<Domain.UserContext.Entities.User>>()))
                .AsImplementedInterfaces();
            builder.RegisterType<DataAccess.Repositories.UserRepository>().AsImplementedInterfaces();
            builder.RegisterType<Domain.UserContext.Services.AppUserManager>().AsSelf();

            var repositoryMiddleware = new Saritasa.Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware(
                new Saritasa.Tools.Messages.Common.Repositories.AdoNetMessageRepository(
                    System.Data.Common.DbProviderFactories.GetFactory(connectionStringConf.ProviderName),
                    connectionString)
            );
            var recordRepositoryMiddleware = new Saritasa.Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware(
                new Saritasa.Tools.Messages.Common.Repositories.WebServiceRepository()
            )
            {
                RethrowExceptions = false,
            };

            var pipelinesContainer = RegisterPipelines();
            builder.RegisterInstance(pipelinesContainer).As<IMessagePipelineContainer>().SingleInstance();
            builder.RegisterType<DefaultPipelineService>().As<IPipelineService>().InstancePerRequest();
            builder.RegisterType<AutofacServiceProvider>().As<IServiceProvider>().InstancePerRequest();

            // Register queries as separate objects.
            builder.RegisterType<Domain.UserContext.Queries.UsersQueries>().AsSelf();
            builder.RegisterType<Domain.TaskContext.Queries.TasksQueries>().AsSelf();
            builder.RegisterType<Domain.TaskContext.Queries.ProjectsQueries>().AsSelf();

            // Emails.
            if (testingMode)
            {
                var emailSender = new Saritasa.Tools.Emails.DummyEmailSender();
                builder.RegisterInstance(emailSender).AsImplementedInterfaces().SingleInstance();
            }
            else
            {
                var emailSender = new Saritasa.Tools.Emails.SmtpClientEmailSender();
                emailSender.AddInterceptor(new FilterEmailInterceptor("*@saritasa.com mytest@example.com"));
                emailSender.AddInterceptor(new CountEmailsInterceptor());
                builder.RegisterInstance(emailSender).AsImplementedInterfaces().SingleInstance();
            }

            // Logger.
            var nlogProvider = new Saritasa.Tools.NLog.NLogLoggerProvider();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(nlogProvider);
            builder.RegisterInstance(loggerFactory).AsImplementedInterfaces().SingleInstance();
        }

        public static IMessagePipelineContainer RegisterPipelines()
        {
            var pipelinesContainer = new SimpleMessagePipelineContainer();

            // Command.
            pipelinesContainer.AddCommandPipeline()
                .AddMiddleware(
                    new Saritasa.Tools.Messages.Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                        System.Reflection.Assembly.GetAssembly(typeof(Domain.UserContext.Entities.User))))
                .AddMiddleware(new Saritasa.Tools.Messages.Commands.PipelineMiddlewares.CommandExecutorMiddleware
                {
                    UseInternalObjectResolver = true,
                    UseParametersResolve = true
                });

            // Query.
            pipelinesContainer.AddQueryPipeline()
                .AddMiddleware(new Saritasa.Tools.Messages.Queries.PipelineMiddlewares.QueryObjectResolverMiddleware
                {
                    UseInternalObjectResolver = true,
                    UseParametersResolve = true,
                })
                .AddMiddleware(new Saritasa.Tools.Messages.Queries.PipelineMiddlewares.QueryExecutorMiddleware())
                .AddMiddleware(new Saritasa.Tools.Messages.Queries.PipelineMiddlewares.QueryObjectReleaseMiddleware());

            // Event.
            pipelinesContainer.AddEventPipeline()
                .AddMiddleware(new Saritasa.Tools.Messages.Events.PipelineMiddlewares.EventHandlerLocatorMiddleware(
                    System.Reflection.Assembly.GetAssembly(typeof(Domain.UserContext.Entities.User))))
                .AddMiddleware(new Saritasa.Tools.Messages.Events.PipelineMiddlewares.EventExecutorMiddleware());
            return pipelinesContainer;
        }
    }
}
