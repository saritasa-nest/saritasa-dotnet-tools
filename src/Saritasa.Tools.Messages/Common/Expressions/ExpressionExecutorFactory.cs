using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using System;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public class ExpressionExecutorFactory
    {
        private IServiceProvider executorServices;

        public ExpressionExecutorFactory(IServiceProvider serviceProvider)
        {
            this.executorServices = serviceProvider;
        }

        public ExpressionExecutor Create()
        {
            var compiledExpressionProvider = executorServices.GetService<ICompiledExpressionCache>();
            var expressionCompilator = executorServices.GetService<IExpressionCompilator>();
            var transformVisitorFactory = executorServices.GetService<IExpressionTransformVisitorFactory>();

            return new ExpressionExecutor(compiledExpressionProvider, expressionCompilator, transformVisitorFactory);
        }
    }
}
