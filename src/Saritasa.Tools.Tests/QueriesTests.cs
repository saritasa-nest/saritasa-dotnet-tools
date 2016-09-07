// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Queries;

    [TestFixture]
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

        class QueryObject
        {
            public IList<int> SimpleQuery(int a, int b)
            {
                return new List<int>() { a, b };
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

        [Test]
        public void Can_run_simple_query()
        {
            var qp = QueryPipeline.CreateDefaultPipeline(QueryPipeline.NullResolver);
            var result = qp.Execute(qp.GetQuery<QueryObject>().SimpleQuery, 10, 20);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[1], Is.EqualTo(20));
        }

        #endregion

        [Test]
        public void Can_run_query_with_resolving()
        {
            var qp = QueryPipeline.CreateDefaultPipeline(QueriesTests.InterfacesResolver);
            var result = qp.Execute(qp.GetQuery<QueryObject>().SimpleQueryWithDependency, 10, 20, (IInterfaceB)null);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[1], Is.EqualTo(20));
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

        [Test]
        public void Can_run_query_with_private_object_ctor()
        {
            var qp = QueryPipeline.CreateDefaultPipeline(QueriesTests.InterfacesResolver);
            var result = qp.Execute(qp.GetQuery<QueryObjectWithPrivateCtor>().Query);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo("A"));
        }

        #endregion
    }
}
