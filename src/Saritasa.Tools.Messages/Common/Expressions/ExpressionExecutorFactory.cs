using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public class ExpressionExecutorFactory
    {
        private IServiceProvider serviceProvider;

        public ExpressionExecutorFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public ExpressionExecutor Create()
        {
            var compiledExpressionProvider = serviceProvider.GetService(typeof(ICompiledExpressionProvider)) as ICompiledExpressionProvider;
            var expressionCompilator = serviceProvider.GetService(typeof(IExpressionCompilator)) as IExpressionCompilator;
            var transformVisitorFactory = serviceProvider.GetService(typeof(IExpressionTransformVisitorFactory)) as IExpressionTransformVisitorFactory;

            return new ExpressionExecutor(compiledExpressionProvider, expressionCompilator, transformVisitorFactory);
        }
    }
}
