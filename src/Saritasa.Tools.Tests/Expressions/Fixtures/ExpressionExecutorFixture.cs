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
            var moq = new Mock<IServiceProvider>();
            moq.Setup(x => x.GetService(It.Is<Type>(t => t == typeof(ICompiledExpressionCache))))
                .Returns(new CompiledExpressionCache());
            moq.Setup(x => x.GetService(It.Is<Type>(t => t == typeof(IExpressionCompilator))))
                .Returns(new ExpressionCompilator());
            moq.Setup(x => x.GetService(It.Is<Type>(t => t == typeof(IExpressionTransformVisitorFactory))))
                .Returns(new ExpressionTransformVisitorFactory(new List<IExpressionTransformer>()
                {
                    new MethodCallExpressionTransformer(),
                    new LambdaExpressionTransformer()
                }));

            var factory = new ExpressionExecutorFactory(moq.Object);

            return factory.Create();
        }
    }
}
