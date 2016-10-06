using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace ZergRushCo.Todosya.Web
{
    public class DiConfig
    {
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
                    c.Resolve<Saritasa.Tools.Commands.ICommandPipeline>()
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

            // command pipeline
            var commandPipeline = Saritasa.Tools.Commands.CommandPipeline.CreateDefaultPipeline(container.Resolve,
                System.Reflection.Assembly.GetAssembly(typeof(Domain.Users.Entities.User)));
            commandPipeline.AppendMiddlewares(
                new Saritasa.Tools.Messages.PipelineMiddlewares.RepositoryMiddleware(
                    new Saritasa.Tools.Messages.Repositories.AdoNetMessageRepository(
                        System.Data.Common.DbProviderFactories.GetFactory(connectionStringConf.ProviderName),
                        connectionString,
                        Saritasa.Tools.Messages.Repositories.AdoNetMessageRepository.Dialect.Sqlite)
                )
            );
            builder = new ContainerBuilder();
            builder.RegisterInstance(commandPipeline).AsImplementedInterfaces().SingleInstance();

            // query pipeline
            var queryPipeline = Saritasa.Tools.Queries.QueryPipeline.CreateDefaultPipeline(container.Resolve);
            builder.RegisterInstance(queryPipeline).AsImplementedInterfaces().SingleInstance();

            // events pipeline
            var eventsPipeline = Saritasa.Tools.Events.EventPipeline.CreateDefaultPipeline(container.Resolve,
                System.Reflection.Assembly.GetAssembly(typeof(Domain.Users.Entities.User)));
            builder.RegisterInstance(eventsPipeline).AsImplementedInterfaces().SingleInstance();

            // register queries as separate objects
            builder.RegisterType<Domain.Users.Queries.UsersQueries>().AsSelf();
            builder.RegisterType<Domain.Tasks.Queries.TasksQueries>().AsSelf();
            builder.RegisterType<Domain.Tasks.Queries.ProjectsQueries>().AsSelf();

            // emails
            var emailSender = new Saritasa.Tools.Emails.SystemMail.SmtpClientEmailSender();
            builder.RegisterInstance(emailSender).AsImplementedInterfaces().SingleInstance();

            // set the dependency resolver to be Autofac
            builder.Update(container);
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
