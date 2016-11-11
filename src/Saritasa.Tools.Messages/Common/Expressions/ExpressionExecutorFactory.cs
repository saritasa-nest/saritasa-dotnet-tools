using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Internal;

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
            var compiledExpressionProvider = serviceProvider.GetService<ICompiledExpressionProvider>();
            var expressionCompilator = serviceProvider.GetService<IExpressionCompilator>();
            var transformVisitorFactory = serviceProvider.GetService<IExpressionTransformVisitorFactory>();

            return new ExpressionExecutor(compiledExpressionProvider, expressionCompilator, transformVisitorFactory);
        }
    }
}
