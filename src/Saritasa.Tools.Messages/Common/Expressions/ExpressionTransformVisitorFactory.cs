using System.Collections.Generic;
using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public class ExpressionTransformVisitorFactory : IExpressionTransformVisitorFactory
    {
        private IReadOnlyList<IExpressionTransformer> expressionTransformers;

        public ExpressionTransformVisitorFactory(IReadOnlyList<IExpressionTransformer> expressionTransformers)
        {
            this.expressionTransformers = expressionTransformers;
        }

        public ExpressionVisitor Create()
        {
            return new ExpressionTransformVisitor(expressionTransformers);
        }
    }
}
