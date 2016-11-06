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
        public ExpressionExecutor Create(Action<ExpressionExecutorContext> configureContext)
        {
            var context = new ExpressionExecutorContext();
            context.TransformContext = new ExpressionTransformContext();
            context.Compilator = new ExpressionCompilator();

            configureContext(context);

            return new ExpressionExecutor(context);
        }
    }
}
