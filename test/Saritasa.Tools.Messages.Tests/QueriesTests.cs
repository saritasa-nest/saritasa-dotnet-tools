// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Queries;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// Message queries tests.
    /// </summary>
    public class QueriesTests
    {
        private readonly IMessagePipelineService pipelineService = new DefaultMessagePipelineService();

        #region Shared interfaces and objects

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

        private void SetupQueryPipeline(QueryPipelineBuilder builder)
        {
            builder
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware())
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware());
        }

        #endregion

        #region QueryWith_SetNumberQuery_NumbersMatch

        [Fact]
        public void QueryWith_SetNumberQuery_NumbersMatch()
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

        #region QueryWithAsync_SetNumberAsyncQuery_NumbersMatch

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
        public async Task QueryWithAsync_SetNumberAsyncQuery_NumbersMatch()
        {
            // Arrange
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());

            // Act
            var result = await pipelineService.Query<AsyncQueryObject>().WithAsync(q => q.GetString());

            // Assert
            Assert.Equal("LP", result);
        }

        #endregion

        #region Invoke_QueryWithDependency_CorrectResultType

        [Fact]
        public void Invoke_QueryWithDependency_CorrectResultType()
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

        #endregion

        #region QueryWith_PrivateQueryObjectCtor_ExecutedWithCorrectNumbers

        private class QueryObjectWithPrivateCtor
        {
            private readonly IInterfaceA dependencyA;
            private readonly IInterfaceB dependencyB;

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
        public void QueryWith_PrivateQueryObjectCtor_ExecutedWithCorrectNumbers()
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

        #region Invoke_MessageContextQueryObject_CorrectResultType

        [Fact]
        public void Invoke_MessageContextQueryObject_CorrectResultType()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(InterfacesResolver);
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());
            var messageContext = new MessageContext(pipelineService);
            var queryPipeline = pipelineService.GetPipelineOfType<IQueryPipeline>();
            queryPipeline.CreateMessageContext<QueryObject>(pipelineService, messageContext)
                .With(q => q.SimpleQuery(10, 10));

            // Act
            queryPipeline.Invoke(messageContext);

            // Assert
            Assert.IsType<List<int>>(messageContext.GetResult<object>());
        }

        #endregion

        #region QueryWith_PipelineWithNoExternalServiceResolver_ValueFilled

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
        public void QueryWith_PipelineWithNoExternalServiceResolver_ValueFilled()
        {
            // Arrange
            pipelineService.PipelineContainer.AddQueryPipeline()
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware(
                    typeof(UserQueries).GetTypeInfo().Assembly))
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware());

            // Act
            var result = pipelineService.Query<IUserQueries>().With(q => q.GetByNameCount("Test"));

            // Assert
            Assert.Equal(34, result);
        }

        #endregion

        #region QueryWith_PipelineWithExternalServiceResolver_ValueFilled

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
        public void QueryWith_PipelineWithExternalServiceResolver_ValueFilled()
        {
            // Arrange
            pipelineService.ServiceProvider = new FuncServiceProvider(ProductQueries.Resolver);
            pipelineService.PipelineContainer.AddQueryPipeline()
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware(
                    typeof(UserQueries).GetTypeInfo().Assembly))
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware());

            // Act
            var result = pipelineService.Query<IProductQueries>().With(q => q.GetByNameCount("Test"));

            // Assert
            Assert.Equal(974, result);
        }

        #endregion

        #region QueryWith_ObjectQueryWithNoDefaultCtor_NoExceptions

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
        public void QueryWith_ObjectQueryWithNoDefaultCtor_NoExceptions()
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

        #region QueryWith_QueryObjectWithException_GetMessageProcessingException

        [QueryHandlers]
        public class QueryObjectWithException
        {
            public string GetString()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void QueryWith_QueryObjectWithException_GetMessageProcessingException()
        {
            // Arrange
            pipelineService.PipelineContainer.AddQueryPipeline().AddStandardMiddlewares();

            // Act & Assert
            Assert.Throws<MessageProcessingException>(() =>
            {
                pipelineService.Query<QueryObjectWithException>().With(q => q.GetString());
            });
        }

        #endregion

        #region QueryWith_ExpressionWithNew_NoExceptions

        public class SimpleQueryObject
        {
            public int DoSomething(DateTime dt) => 2;
        }

        [Fact]
        public void QueryWith_ExpressionWithNew_NoExceptions()
        {
            // Arrange
            SetupQueryPipeline(pipelineService.PipelineContainer.AddQueryPipeline());

            // Act
            var result = pipelineService.Query<SimpleQueryObject>().With(q => q.DoSomething(new DateTime(2018, 9, 20)));

            // Assert
            Assert.Equal(2, result);
        }

        #endregion
    }
}
