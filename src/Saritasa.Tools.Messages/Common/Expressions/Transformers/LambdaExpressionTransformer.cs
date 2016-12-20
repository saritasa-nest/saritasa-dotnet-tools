using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions.Transformers
{
    /// <summary>
    /// Transformer for lambda expression.
    /// </summary>
    public class LambdaExpressionTransformer : IExpressionTransformer
    {
        /// <inheritdoc/>
        public bool SupportTransform(ExpressionType nodeFrom)
        {
            return nodeFrom == ExpressionType.Lambda;
        }

        /// <inheritdoc/>
        public Expression Transform(Expression input, ExpressionTransformVisitor visitor)
        {
            if (visitor.TransformedParameterExpressions.Count == 0)
            {
                return input;
            }

            var lamdaExpression = input as LambdaExpression;
            if (lamdaExpression == null)
            {
                throw new ArgumentException("Can't convert input expression to lambda expression.");
            }

            var parameters = new List<ParameterExpression>(lamdaExpression.Parameters.Count + visitor.TransformedParameterExpressions.Count);
            parameters.AddRange(lamdaExpression.Parameters);
            parameters.AddRange(visitor.TransformedParameterExpressions);

            return Expression.Lambda(lamdaExpression.Body, parameters);
        }
    }
}
