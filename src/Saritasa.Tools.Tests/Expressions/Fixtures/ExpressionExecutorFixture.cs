using Moq;
using Saritasa.Tools.Messages.Common.Expressions;
using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using Saritasa.Tools.Messages.Common.Expressions.Transformers;
using System;
using System.Collections.Generic;

namespace Saritasa.Tools.Tests.Expressions.Fixtures
{
    public class ExpressionExecutorFixture
    {
        public ExpressionExecutor Create()
        {
            var factory = new ExpressionExecutorFactory(new ExpressionExecutorServices());

            return factory.Create();
        }
    }
}
