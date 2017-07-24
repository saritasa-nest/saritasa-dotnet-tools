// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
        readonly IPipelineService pipelineService = new DefaultPipelineService();

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
                if (dependencyB == null)
                {
                    return null;
                }
                return new List<int> { a, b };
            }
        }

        public void SetupQueryPipeline(QueryPipelineBuilder builder)
        {
            builder
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware()
                {
                    UseInternalObjectResolver = true,
                    UseParametersResolve = true,
                })
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware())
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectReleaseMiddleware());
        }

        #region Can_run_simple_query

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

        #endregion

        [Fact]
        public void Can_run_query_with_resolving()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(InterfacesResolver);
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());

            // Act
            var result = pipelineService.Query<QueryObject>().With(q => q.SimpleQueryWithDependency(10, 20, null));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(20, result[1]);
        }

        [Fact]
        public void Can_run_query_from_raw_message()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(InterfacesResolver);
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());
            var messageRecord = new MessageRecord
            {
                ContentType = "Saritasa.Tools.Messages.Tests.QueriesTests+QueryObject.SimpleQueryWithDependency",
                Content = new Dictionary<string, object>
                {
                    ["a"] = 10,
                    ["b"] = 20,
                    ["dependencyB"] = null,
                }
            };

            // Act
            var queryPipeline = pipelineService.GetPipelineOfType<IQueryPipeline>();
            var messageContext = queryPipeline.CreateMessageContext(pipelineService, messageRecord);
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
            var ret = queryPipeline.CreateMessageContext<QueryObject>(pipelineService, messageContext)
                .With(q => q.SimpleQuery(10, 10));

            // Act
            queryPipeline.Invoke(messageContext);

            // Assert
            var result = messageContext.GetResult<object>();
            Assert.IsType<List<int>>(result);
        }

        #endregion
    }
}
