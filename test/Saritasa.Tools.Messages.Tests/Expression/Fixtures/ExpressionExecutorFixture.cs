using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Common.Expressions;

namespace Saritasa.Tools.Messages.Tests.Expression.Fixtures
{
    public class ExpressionExecutorFixture
    {
        public ExpressionExecutor Create()
        {
            var factory = new ExpressionExecutorFactory(new ExpressionExecutorServices());

            return factory.Create();
        }

        public TestClass CreateTestObject()
        {
            return new TestClass()
            {
                Value1 = 1,
                Value2 = 2
            };
        }

        public class TestClass
        {
            public int Value1 { get; set; }

            public int Value2 { get; set; }
        }
    }
}
