using System;
using System.Reflection;
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

namespace SandBox
{
    class Program
    {
        public static ICommandPipeline CommandPipeline { get; private set; }

        public static IQueryPipeline QueryPipeline { get; private set; }

        public static IEventPipeline EventPipeline { get; private set; }

        static InMemoryMessageRepository inMemoryMessageRepository;

        static IProductsRepository productsRepository = new ProductsRepository();

        /// <summary>
        /// Simple dependency injection resolver.
        /// </summary>
        /// <param name="type">Type to find object.</param>
        /// <returns>Instaniated object.</returns>
        static object Resolver(Type type)
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
            // we will use in memory repository
            inMemoryMessageRepository = new InMemoryMessageRepository();

            // create command pipeline manually
            CommandPipeline = new CommandPipeline();
            CommandPipeline.AppendMiddlewares(
                new CommandValidationMiddleware(),
                new CommandHandlerLocatorMiddleware(Assembly.GetEntryAssembly()),
                new CommandExecutorMiddleware(Resolver),
                new RepositoryMiddleware(inMemoryMessageRepository)
            );
            CommandPipeline.UseInternalResolver(true);

            // create query pipeline manually
            QueryPipeline = new QueryPipeline();
            QueryPipeline.AppendMiddlewares(
                new QueryObjectResolverMiddleware(Resolver),
                new QueryExecutorMiddleware(),
                new RepositoryMiddleware(inMemoryMessageRepository)
            );
            QueryPipeline.UseInternalResolver(true);

            // create event pipeline manually
            EventPipeline = new EventPipeline();
            EventPipeline.AppendMiddlewares(
                new EventHandlerLocatorMiddleware(Assembly.GetEntryAssembly()),
                new EventExecutorMiddleware(Resolver),
                new RepositoryMiddleware(inMemoryMessageRepository)
            );
            EventPipeline.UseInternalResolver(true);
        }

        static void Test()
        {
            // command
            CommandPipeline.Handle(new UpdateProductCommand()
            {
                ProductId = 10,
                Name = "Test",
            });
            Console.WriteLine(inMemoryMessageRepository.Dump());
            inMemoryMessageRepository.Messages.Clear();

            // query
            var list = ((QueryPipeline)QueryPipeline).Query<ProductsQueries>().With(q => q.Get(0, 10));
            Console.WriteLine(inMemoryMessageRepository.Dump());
            inMemoryMessageRepository.Messages.Clear();

            // event
            EventPipeline.Raise(new UpdateProductEvent() { Id = 1 });
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
