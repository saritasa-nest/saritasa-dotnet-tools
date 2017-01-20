using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using Saritasa.Tools.Messages.Commands;
using Saritasa.Tools.Messages.Queries;
using Saritasa.Tools.Messages.Events;
using Saritasa.Tools.Emails.Interceptors;
using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.Logging;
using ZergRushCo.Todosya.DataAccess;

namespace ZergRushCo.Todosya.Web
{
    /// <summary>
    /// Dependency injection configuration.
    /// </summary>
    public class DiConfig
    {
        /// <summary>
        /// Configures dependency injection container.
        /// </summary>
        public static void Register()
        {
            var builder = new ContainerBuilder();

            // Register MVC controllers.
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // Enable property injection into action filters.
            builder.RegisterFilterProvider();

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
            builder.Register(
                c => new Microsoft.AspNet.Identity.Owin.SignInManager<Domain.UserContext.Entities.User, string>(
                    c.Resolve<Domain.UserContext.Services.AppUserManager>(),
                    HttpContext.Current.GetOwinContext().Authentication));
            builder.RegisterType<DataAccess.Repositories.UserRepository>().AsImplementedInterfaces();
            builder.RegisterType<Domain.UserContext.Services.AppUserManager>().AsSelf();

            var repositoryMiddleware = new Saritasa.Tools.Messages.Common.PipelineMiddlewares.RepositoryMiddleware(
                new Saritasa.Tools.Messages.Common.Repositories.AdoNetMessageRepository(
                    System.Data.Common.DbProviderFactories.GetFactory(connectionStringConf.ProviderName),
                    connectionString)
            );

            // Command pipeline.
            builder.Register(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    var commandPipeline = CommandPipeline.CreateDefaultPipeline(context.Resolve,
                        System.Reflection.Assembly.GetAssembly(typeof(Domain.UserContext.Entities.User)));
                    commandPipeline.AppendMiddlewares(repositoryMiddleware);
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

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
