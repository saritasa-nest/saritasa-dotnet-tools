using System;
using System.Collections.Concurrent;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Commands;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Benchmark
{
    /// <summary>
    /// Commands pipeline benchmarks.
    /// </summary>
    public class CommandsBenchmarks
    {
        const int NumberOfInterations = 100000;

        #region Interfaces

        public interface IUowFactory
        {
            object Create();
        }

        public class UowFactory : IUowFactory
        {
            public object Create() => new object();
        }

        public interface IUsersService
        {
            string GetSsn(string firstName, string lastName, DateTime dob);
        }

        public class ExternalUsersService : IUsersService
        {
            public string GetSsn(string firstName, string lastName, DateTime dob) => "123-123-123";
        }

        public static object InterfacesResolver(Type t)
        {
            if (t == typeof(IUowFactory))
            {
                return new UowFactory();
            }
            else if (t == typeof(IUsersService))
            {
                return new ExternalUsersService();
            }
            return null;
        }

        #endregion

        public class CreateUserCommand
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public DateTime BirthDay { get; set; }

            [CommandOut]
            public int Id { get; set; }
        }

        [CommandHandlers]
        public sealed class UsersCommandHandlers
        {
            readonly IUsersService usersService;

            public UsersCommandHandlers(IUsersService usersService)
            {
                if (usersService == null)
                {
                    throw new ArgumentNullException(nameof(usersService));
                }
                this.usersService = usersService;
            }

            public void HandleCreateUser(CreateUserCommand command, IUowFactory uowFactory)
            {
                if (uowFactory == null)
                {
                    throw new ArgumentNullException(nameof(uowFactory));
                }
                command.Id = 1;
            }
        }

        //[Benchmark(Baseline = true)]
        public void RunCommandDirect()
        {
            var usersCommand = new UsersCommandHandlers((IUsersService)InterfacesResolver(typeof(IUsersService)));
            var uowFactory = (IUowFactory)InterfacesResolver(typeof(IUowFactory));
            for (int i = 0; i < NumberOfInterations; i++)
            {
                var cmd = new CreateUserCommand()
                {
                    FirstName = "Ivan",
                    LastName = "Ivanov",
                    BirthDay = new DateTime(1985, 1, 1),
                };
                usersCommand.HandleCreateUser(cmd, uowFactory);
            }
        }

        [Benchmark]
        public void RunCommandWithPipeline()
        {
            var piplinesService = new DefaultPipelinesService();
            piplinesService.ServiceProvider = new FuncServiceProvider(InterfacesResolver);
            piplinesService.AddCommandPipeline()
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandHandlerLocatorMiddleware(
                    typeof(CreateUserCommand).GetTypeInfo().Assembly))
                .AddMiddleware(new Commands.PipelineMiddlewares.CommandExecutorMiddleware
                {
                    UseInternalObjectResolver = true,
                    UseParametersResolve = true
                });

            for (int i = 0; i < NumberOfInterations; i++)
            {
                var cmd = new CreateUserCommand
                {
                    FirstName = "Ivan",
                    LastName = "Ivanov",
                    BirthDay = new DateTime(1985, 1, 1),
                };
                piplinesService.HandleCommand(cmd);
            }
        }
    }
}
