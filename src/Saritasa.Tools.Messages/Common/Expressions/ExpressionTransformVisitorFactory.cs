using System.Collections.Generic;
using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    /// <inheritdoc/>
    public class ExpressionTransformVisitorFactory : IExpressionTransformVisitorFactory
    {
        private IReadOnlyList<IExpressionTransformer> expressionTransformers;

        /// <summary>
        /// Ctor.
        /// </summary>
        public ExpressionTransformVisitorFactory(IReadOnlyList<IExpressionTransformer> expressionTransformers)
        {
            this.expressionTransformers = expressionTransformers;
        }

        /// <inheritdoc/>
        public ExpressionVisitor Create()
        {
            return new ExpressionTransformVisitor(expressionTransformers);
        }
    }
}
