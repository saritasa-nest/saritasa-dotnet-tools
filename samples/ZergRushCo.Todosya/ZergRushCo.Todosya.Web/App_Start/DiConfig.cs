using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using Saritasa.Tools.Commands;
using Saritasa.Tools.Queries;
using Saritasa.Tools.Events;
using Saritasa.Tools.Emails.Interceptors;

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

            // register MVC controllers
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // register web abstractions like HttpContextBase
            builder.RegisterModule<AutofacWebTypesModule>();

            // enable property injection in view pages
            builder.RegisterSource(new ViewRegistrationSource());

            // enable property injection into action filters
            builder.RegisterFilterProvider();

            // other bindings
            var connectionStringConf = ConfigurationManager.ConnectionStrings["AppDbContext"];
            var connectionString = connectionStringConf.ConnectionString.Replace("{baseUrl}",
                AppDomain.CurrentDomain.BaseDirectory);
            builder.RegisterType<DataAccess.AppUnitOfWork>().AsImplementedInterfaces();
            builder.RegisterType<DataAccess.AppUnitOfWorkFactory>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<DataAccess.AppDbContext>().AsSelf();
            builder.Register(c =>
                new Domain.Users.Services.UserStoreService(
                    c.Resolve<Domain.Users.Repositories.IUserRepository>(),
                    c.Resolve<ICommandPipeline>()
                )).AsImplementedInterfaces();
            builder.Register(c =>
                new Core.Identity.AppSignInManager(
                    c.Resolve<Domain.Users.Services.AppUserManager>(),
                    HttpContext.Current.GetOwinContext().Authentication
                )).AsSelf();
            builder.RegisterType<DataAccess.Repositories.UserRepository>().AsImplementedInterfaces();
            builder.RegisterType<Domain.Users.Services.AppUserManager>().AsSelf();

            // make container
            var container = builder.Build();

            var repositoryMiddleware = new Saritasa.Tools.Messages.PipelineMiddlewares.RepositoryMiddleware(
                new Saritasa.Tools.Messages.Repositories.AdoNetMessageRepository(
                    System.Data.Common.DbProviderFactories.GetFactory(connectionStringConf.ProviderName),
                    connectionString)
            );

            // command pipeline
            var commandPipeline = CommandPipeline.CreateDefaultPipeline(container.Resolve,
                System.Reflection.Assembly.GetAssembly(typeof(Domain.Users.Entities.User)));
            commandPipeline.AppendMiddlewares(repositoryMiddleware);
            commandPipeline.UseInternalResolver(true);
            builder = new ContainerBuilder();
            builder.RegisterInstance(commandPipeline).AsImplementedInterfaces().SingleInstance();

            // query pipeline
            var queryPipeline = QueryPipeline.CreateDefaultPipeline(container.Resolve);
            queryPipeline.AppendMiddlewares(repositoryMiddleware);
            queryPipeline.UseInternalResolver(true);
            builder.RegisterInstance(queryPipeline).AsImplementedInterfaces().SingleInstance();

            // events pipeline
            var eventsPipeline = EventPipeline.CreateDefaultPipeline(container.Resolve,
                System.Reflection.Assembly.GetAssembly(typeof(Domain.Users.Entities.User)));
            eventsPipeline.UseInternalResolver(true);
            builder.RegisterInstance(eventsPipeline).AsImplementedInterfaces().SingleInstance();

            // register queries as separate objects
            builder.RegisterType<Domain.Users.Queries.UsersQueries>().AsSelf();
            builder.RegisterType<Domain.Tasks.Queries.TasksQueries>().AsSelf();
            builder.RegisterType<Domain.Tasks.Queries.ProjectsQueries>().AsSelf();

            // emails
            var emailSender = new Saritasa.Tools.Emails.SmtpClientEmailSender();
            emailSender.AddInterceptor(new FilterEmailInterceptor("*@saritasa.com mytest@example.com"));
            emailSender.AddInterceptor(new CountEmailsInterceptor());
            builder.RegisterInstance(emailSender).AsImplementedInterfaces().SingleInstance();

            // logger
            var loggerFactory = new Saritasa.Tools.NLog.LoggerFactory();
            builder.RegisterInstance(loggerFactory).AsImplementedInterfaces().SingleInstance();

            // set the dependency resolver to be Autofac
            builder.Update(container);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
