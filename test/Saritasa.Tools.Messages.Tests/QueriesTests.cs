// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Tests
{
    using System;
    using System.Collections.Generic;
    using Xunit;
    using Common;
    using Queries;

    /// <summary>
    /// Message queries tests.
    /// </summary>
    public class QueriesTests
    {
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
                return new List<int>() { a, b };
            }
        }

        #region Can_run_simple_query

        [Fact]
        public void Can_run_simple_query()
        {
            // Arrange
            var qp = QueryPipeline.CreateDefaultPipeline(QueryPipeline.NullResolver).UseInternalResolver(true);

            // Act
            var result = qp.Query<QueryObject>().With(q => q.SimpleQuery(10, 20));

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
            var qp = QueryPipeline.CreateDefaultPipeline(QueriesTests.InterfacesResolver).UseInternalResolver();

            // Act
            var result = qp.Query<QueryObject>().With(q => q.SimpleQueryWithDependency(10, 20, null));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(20, result[1]);
        }

        [Fact]
        public void Can_run_query_from_raw_message()
        {
            // Arrange
            var qp = QueryPipeline.CreateDefaultPipeline(QueriesTests.InterfacesResolver).UseInternalResolver();
            var message = new Message()
            {
                ContentType = "Saritasa.Tools.Messages.Tests.QueriesTests+QueryObject.SimpleQueryWithDependency",
                Type = Message.MessageTypeQuery,
                Content = new Dictionary<string, object>()
                {
                    ["a"] = 10,
                    ["b"] = 20,
                    ["dependencyB"] = null,
                }
            };

            // Act
            qp.ProcessRaw(message);

            // Assert
            Assert.IsType<List<int>>(message.Content);
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
                return new List<string>() { dependencyA.GetTestValue(), dependencyB.GetTestValue() };
            }
        }

        [Fact]
        public void Can_run_query_with_private_object_ctor()
        {
            // Arrange
            var qp = QueryPipeline.CreateDefaultPipeline(QueriesTests.InterfacesResolver).UseInternalResolver();

            // Act
            var result = qp.Query<QueryObjectWithPrivateCtor>().With(q => q.Query());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("A", result[0]);
        }

        #endregion
    }
}
