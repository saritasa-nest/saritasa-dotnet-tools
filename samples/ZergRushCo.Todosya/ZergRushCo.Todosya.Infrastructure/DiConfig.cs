using System;
using System.Configuration;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Emails.Interceptors;
using Saritasa.Tools.Messages.Commands;
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
        public static void Setup(ContainerBuilder builder)
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

            // Command pipeline.
            builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                var commandPipeline = CommandPipeline.CreateDefaultPipeline(context.Resolve,
                    System.Reflection.Assembly.GetAssembly(typeof(Domain.UserContext.Entities.User)));
                commandPipeline.AppendMiddlewares(repositoryMiddleware);
                commandPipeline.AppendMiddlewares(recordRepositoryMiddleware);
                commandPipeline.UseInternalResolver();
                builder = new ContainerBuilder();

                return commandPipeline;
            }).AsImplementedInterfaces().SingleInstance();

            // Query pipeline.
            builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                var queryPipeline = QueryPipeline.CreateDefaultPipeline(context.Resolve);
                queryPipeline.AppendMiddlewares(repositoryMiddleware);
                queryPipeline.UseInternalResolver();
                builder.RegisterInstance(queryPipeline).AsImplementedInterfaces().SingleInstance();

                return queryPipeline;
            }).AsImplementedInterfaces().SingleInstance();

            // Events pipeline.
            builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                var eventsPipeline = EventPipeline.CreateDefaultPipeline(context.Resolve,
                    System.Reflection.Assembly.GetAssembly(typeof(Domain.UserContext.Entities.User)));
                eventsPipeline.UseInternalResolver();

                return eventsPipeline;
            }).AsImplementedInterfaces().SingleInstance();

            // Register queries as separate objects.
            builder.RegisterType<Domain.UserContext.Queries.UsersQueries>().AsSelf();
            builder.RegisterType<Domain.TaskContext.Queries.TasksQueries>().AsSelf();
            builder.RegisterType<Domain.TaskContext.Queries.ProjectsQueries>().AsSelf();

            // Emails.
            var emailSender = new Saritasa.Tools.Emails.SmtpClientEmailSender();
            emailSender.AddInterceptor(new FilterEmailInterceptor("*@saritasa.com mytest@example.com"));
            emailSender.AddInterceptor(new CountEmailsInterceptor());
            builder.RegisterInstance(emailSender).AsImplementedInterfaces().SingleInstance();

            // Logger.
            var nlogProvider = new Saritasa.Tools.NLog.NLogLoggerProvider();
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(nlogProvider);
            builder.RegisterInstance(loggerFactory).AsImplementedInterfaces().SingleInstance();
        }
    }
}
