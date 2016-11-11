using Moq;
using Saritasa.Tools.Messages.Common.Expressions;
using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using Saritasa.Tools.Messages.Common.Expressions.Transformers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Tests.Expressions.Fixtures
{
    public class ExpressionExecutorFixture
    {
        public ExpressionExecutor Create()
        {
            var moq = new Mock<IServiceProvider>();
            moq.Setup(x => x.GetService(It.Is<Type>(t => t == typeof(ICompiledExpressionProvider))))
                .Returns(new CompiledExpressionProvider());
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
