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
            return transformers.FirstOrDefault(transfomer => transfomer.SupportTransform(node.NodeType));
        }

        private bool HasSupportingTransformer(Expression node)
        {
            return transformers.Any(transformer => transformer.SupportTransform(node.NodeType));
        }

        /// <inheritdoc/>
        public override Expression Visit(Expression node)
        {
            var visitedExpression = base.Visit(node);

            return visitedExpression;
        }

        /// <inheritdoc/>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }

        /// <inheritdoc/>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var visitedLambda = base.VisitLambda<T>(node);

            if (HasSupportingTransformer(node))
            {
                var transformer = GetTransfomer(node);
                return transformer.Transform(visitedLambda, this);
            }

            return visitedLambda;
        }

        /// <inheritdoc/>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (HasSupportingTransformer(node))
            {
                var transfomer = GetTransfomer(node);
                return transfomer.Transform(node, this);
            }

            return base.VisitMethodCall(node);
        }
    }
}
