using System;
using System.Linq;
using System.Reflection;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Commands;
using Saritasa.Tools.Messages.Commands.PipelineMiddlewares;
using Saritasa.Tools.Messages.Events;
using Saritasa.Tools.Messages.Events.PipelineMiddlewares;
using Saritasa.Tools.Messages.Common.PipelineMiddlewares;
using Saritasa.Tools.Messages.Common.Repositories;
using Saritasa.Tools.Messages.Queries;
using Saritasa.Tools.Messages.Queries.PipelineMiddlewares;
using SandBox.Commands;
using SandBox.Events;
using SandBox.Queries;
using Saritasa.Tools.Messages.Abstractions.Commands;
using Saritasa.Tools.Messages.Abstractions.Events;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Common;

namespace SandBox
{
    class Program
    {
        public static ICommandPipeline CommandPipeline { get; private set; }

        public static IQueryPipeline QueryPipeline { get; private set; }

        public static IEventPipeline EventPipeline { get; private set; }

        private static InMemoryMessageRepository inMemoryMessageRepository;

        private static readonly IProductsRepository productsRepository = new ProductsRepository();

        /// <summary>
        /// Simple dependency injection resolver.
        /// </summary>
        /// <param name="type">Type to find object.</param>
        /// <returns>Instaniated object.</returns>
        public static object Resolver(Type type)
        {
            if (type == typeof(IProductsRepository))
            {
                return productsRepository;
            }
            return null;
        }

        /// <summary>
        /// Demo init.
        /// </summary>
        static void Init()
        {
            Paging.Try1();

            /*
            var config = Saritasa.Tools.Messages.Configuration.XmlConfiguration.AppConfig;
            CommandPipeline = (ICommandPipeline)config.First();
            */

            // We will use in memory repository.
            inMemoryMessageRepository = new InMemoryMessageRepository();

            // Create command pipeline manually.
            CommandPipeline = new CommandPipeline();
            CommandPipeline.AddMiddlewares(
                new CommandValidationMiddleware(),
                new CommandHandlerLocatorMiddleware(Assembly.GetEntryAssembly()),
                new CommandExecutorMiddleware(),
                new RepositoryMiddleware(inMemoryMessageRepository)
            );

            // Create query pipeline manually.
            QueryPipeline = new QueryPipeline();
            QueryPipeline.AddMiddlewares(
                new QueryObjectResolverMiddleware(),
                new QueryExecutorMiddleware(),
                new RepositoryMiddleware(inMemoryMessageRepository)
            );

            // Create event pipeline manually.
            EventPipeline = new EventPipeline();
            EventPipeline.AddMiddlewares(
                new EventHandlerLocatorMiddleware(Assembly.GetEntryAssembly()),
                new EventExecutorMiddleware(),
                new RepositoryMiddleware(inMemoryMessageRepository)
            );
        }

        static void Test()
        {
            var pipelineService = new DefaultMessagePipelineService();
            pipelineService.ServiceProvider = new FuncServiceProvider(Resolver);
            pipelineService.PipelineContainer = new DefaultMessagePipelineContainer
            {
                Pipelines = new IMessagePipeline[]
                {
                    CommandPipeline,
                    QueryPipeline,
                    EventPipeline
                }
            };

            // Command.
            pipelineService.HandleCommand(new UpdateProductCommand
            {
                ProductId = 10,
                Name = "Test",
            });
            Console.WriteLine(inMemoryMessageRepository.Dump());
            inMemoryMessageRepository.Messages.Clear();

            // Query.
            var list = pipelineService.Query<ProductsQueries>().With(q => q.Get(0, 10));
            Console.WriteLine(inMemoryMessageRepository.Dump());
            inMemoryMessageRepository.Messages.Clear();

            // Event.
            pipelineService.RaiseEvent(new UpdateProductEvent { Id = 1 });
            Console.WriteLine(inMemoryMessageRepository.Dump());
            inMemoryMessageRepository.Messages.Clear();
        }

        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Arguments.</param>
        static void Main(string[] args)
        {
            Init();
            Test();
            LoggingBox.Test();
            Console.ReadKey();
        }
    }
}
