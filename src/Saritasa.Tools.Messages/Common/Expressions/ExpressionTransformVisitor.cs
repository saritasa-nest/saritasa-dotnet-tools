using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    /// <summary>
    /// Visitor for apply transform.
    /// </summary>
    /// <remarks>
    /// For other code actions just override protected internal methods for hook.
    /// </remarks>
    public class ExpressionTransformVisitor : ExpressionVisitor
    {
        private IReadOnlyList<IExpressionTransformer> transformers;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="transformers"></param>
        public ExpressionTransformVisitor(IReadOnlyList<IExpressionTransformer> transformers)
        {
            this.transformers = transformers;
        }

        /// <summary>
        /// Transformed parameters
        /// </summary>
        public ICollection<ParameterExpression> TransformedParameterExpressions { get; set; } = new List<ParameterExpression>();

        private IExpressionTransformer GetTransfomer(Expression node)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < transformers.Count; i++)
            {
                var transformer = transformers[i];
                if (transformer.SupportTransform(node.NodeType))
                {
                    return transformer;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override Expression Visit(Expression node)
        {
            var visitedExpression = base.Visit(node);

            return visitedExpression;
        }

        /// <inheritdoc/>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var visitedLambda = base.VisitLambda<T>(node);

            var transformer = GetTransfomer(node);
            if (transformer != null)
            {
                return transformer.Transform(visitedLambda, this);
            }

            return visitedLambda;
        }

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var transfomer = GetTransfomer(node);
            if (transfomer != null)
            {
                return transfomer.Transform(node, this);
            }

            return base.VisitMethodCall(node);
        }
    }
}
