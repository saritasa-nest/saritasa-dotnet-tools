using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
        private ExpressionTransformContext context;

        public ExpressionTransformVisitor(ExpressionTransformContext context)
        {
            this.context = context;
        }

        public ExpressionTransformContext Context => context;

        private IExpressionTransformer GetTransfomer(Expression node)
        {
            return context.Transformers.FirstOrDefault(transfomer => transfomer.SupportTransform(node.NodeType));
        }

        public override Expression Visit(Expression node)
        {
            var visitedExpression = base.Visit(node);

            return visitedExpression;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var visitedLambda = base.VisitLambda<T>(node);

            if (context.HasSupportingTransformer(node))
            {
                var transformer = GetTransfomer(node);
                return transformer.Transform(visitedLambda, context);
            }

            return visitedLambda;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (context.HasSupportingTransformer(node))
            {
                var transfomer = GetTransfomer(node);
                return transfomer.Transform(node, context);
            }

            return base.VisitMethodCall(node);
        }
    }
}
