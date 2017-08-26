// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Xunit;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Queries;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// Message queries tests.
    /// </summary>
    public class QueriesTests
    {
        readonly IMessagePipelineService pipelineService = new DefaultMessagePipelineService();

        #region Interfaces

        public interface IInterfaceA
        {
            string GetTestValue();
        }

        public class ImplementationA : IInterfaceA
        {
            public string GetTestValue() => "A";
        }

        public interface IInterfaceB
        {
            string GetTestValue();
        }

        public class ImplementationB : IInterfaceB
        {
            public string GetTestValue() => "B";
        }

        public static object InterfacesResolver(Type t)
        {
            if (t == typeof(IInterfaceA))
            {
                return new ImplementationA();
            }
            else if (t == typeof(IInterfaceB))
            {
                return new ImplementationB();
            }
            return null;
        }

        #endregion

        public class QueryObject
        {
            public IList<int> SimpleQuery(int a, int b)
            {
                return new List<int> { a, b };
            }

            public IList<int> SimpleQueryWithDependency(int a, int b, IInterfaceB dependencyB)
            {
                return new List<int> { a, b };
            }
        }

        public void SetupQueryPipeline(QueryPipelineBuilder builder)
        {
            builder
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware()
                {
                    UseInternalObjectResolver = true
                })
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware())
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectReleaseMiddleware());
        }

        [Fact]
        public void Can_run_simple_query()
        {
            // Arrange
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());

            // Act
            var result = pipelineService.Query<QueryObject>().With(q => q.SimpleQuery(10, 20));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(20, result[1]);
        }

        #region Can_run_simple_async_query

        [QueryHandlers]
        public class AsyncQueryObject
        {
            public async Task<string> GetString()
            {
                await Task.Delay(100);
                return await Task.FromResult("LP");
            }
        }

        [Fact]
        public async Task Can_run_simple_async_query()
        {
            // Arrange
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());

            // Act
            var result = await pipelineService.Query<AsyncQueryObject>().WithAsync(q => q.GetString());

            // Assert
            Assert.Equal("LP", result);
        }

        #endregion

        [Fact]
        public void Can_run_query_from_raw_message()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(InterfacesResolver);
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());
            var messageRecord = new MessageRecord
            {
                ContentType = "Saritasa.Tools.Messages.Tests.QueriesTests+QueryObject.SimpleQueryWithDependency," +
                    "Saritasa.Tools.Messages.Tests",
                Content = new Dictionary<string, object>
                {
                    ["a"] = 10,
                    ["b"] = 20,
                    ["dependencyB"] = null,
                }
            };

            // Act
            var queryPipeline = pipelineService.GetPipelineOfType<IQueryPipeline>();
            var queryConverter = pipelineService.GetPipelineOfType<IQueryPipeline>() as IMessageRecordConverter;
            var messageContext = queryConverter.CreateMessageContext(pipelineService, messageRecord);
            queryPipeline.Invoke(messageContext);

            // Assert
            Assert.IsType<List<int>>(messageContext.GetResult<object>());
        }

        #region Can_run_query_with_private_object_ctor

        class QueryObjectWithPrivateCtor
        {
            readonly IInterfaceA dependencyA;
            readonly IInterfaceB dependencyB;

            private QueryObjectWithPrivateCtor()
            {
            }

            public QueryObjectWithPrivateCtor(IInterfaceA dependencyA, IInterfaceB dependencyB)
            {
                this.dependencyA = dependencyA;
                this.dependencyB = dependencyB;
            }

            public IList<string> Query()
            {
                return new List<string> { dependencyA.GetTestValue(), dependencyB.GetTestValue() };
            }
        }

        [Fact]
        public void Can_run_query_with_private_object_ctor()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(InterfacesResolver);
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());

            // Act
            var result = pipelineService.Query<QueryObjectWithPrivateCtor>().With(q => q.Query());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("A", result[0]);
        }

        #endregion

        #region Can_run_query_from_raw_message_2

        [Fact]
        public void Can_run_query_from_raw_message_2()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(InterfacesResolver);
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());
            var messageContext = new MessageContext(pipelineService);
            var queryPipeline = pipelineService.GetPipelineOfType<IQueryPipeline>();
            var ret = queryPipeline.CreateMessageContext<QueriesTests.QueryObject>(pipelineService, messageContext)
                .With(q => q.SimpleQuery(10, 10));

            // Act
            queryPipeline.Invoke(messageContext);

            // Assert
            var result = messageContext.GetResult<object>();
            Assert.IsType<List<int>>(result);
        }

        #endregion

        #region Can_use_interfaces_to_run_query

        public interface IUserQueries
        {
            int GetByNameCount(string name);
        }

        [QueryHandlers]
        public class UserQueries : IUserQueries
        {
            public int GetByNameCount(string name) => 34;
        }

        [Fact]
        public void Can_use_interfaces_to_run_query()
        {
            // Arrange
            pipelineService.PipelineContainer.AddQueryPipeline()
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware(
                    typeof(UserQueries).GetTypeInfo().Assembly)
                {
                    UseInternalObjectResolver = true
                })
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware())
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectReleaseMiddleware());

            // Act
            var result = pipelineService.Query<IUserQueries>().With(q => q.GetByNameCount("Test"));

            // Assert
            Assert.Equal(34, result);
        }

        #endregion

        #region Should_use_interface_in_case_of_external_resolver

        public interface IProductQueries
        {
            int GetByNameCount(string name);
        }

        [QueryHandlers]
        public class ProductQueries : IProductQueries
        {
            public int GetByNameCount(string name) => 974;

            public static object Resolver(Type type)
            {
                if (type == typeof(IProductQueries))
                {
                    return new ProductQueries();
                }
                return null;
            }
        }

        [Fact]
        public void Should_use_interface_in_case_of_external_resolver()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(ProductQueries.Resolver);
            pipelineService.PipelineContainer.AddQueryPipeline()
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware(
                    typeof(UserQueries).GetTypeInfo().Assembly)
                {
                    UseInternalObjectResolver = false,
                })
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware())
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectReleaseMiddleware());

            // Act
            var result = pipelineService.Query<IProductQueries>().With(q => q.GetByNameCount("Test"));

            // Assert
            Assert.Equal(974, result);
        }

        #endregion

        #region Query object should not require parameterless ctor

        public class UserQueries2Dep
        {
            public string Name { get; } = "Quake Champions";
        }

        [QueryHandlers]
        public class UserQueries2
        {
            private readonly UserQueries2Dep dep;

            public UserQueries2(UserQueries2Dep dep)
            {
                this.dep = dep;
            }

            public string GetString() => dep.Name;
        }

        [Fact]
        public void Query_object_should_not_require_parameterles_ctor()
        {
            // Arrange
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());
            pipelineService.ServiceProvider = new FuncServiceProvider(Activator.CreateInstance);

            // Act
            var result = pipelineService.Query<UserQueries2>().With(q => q.GetString());

            // Assert
            Assert.Equal("Quake Champions", result);
        }

        #endregion
    }
}
