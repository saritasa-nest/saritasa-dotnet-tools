using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    /// <summary>
    /// Context for expression transformation.
    /// </summary>
    public class ExpressionTransformContext
    {
        private IList<IExpressionTransformer> transformers = new List<IExpressionTransformer>();

        /// <summary>
        /// Transformers of expression.
        /// </summary>
        public IReadOnlyCollection<IExpressionTransformer> Transformers => new ReadOnlyCollection<IExpressionTransformer>(transformers);

        /// <summary>
        /// Transformed parameter expressions.
        /// </summary>
        public ICollection<ParameterExpression> TransformedParameterExpressions { get; set; } = new List<ParameterExpression>();

        /// <summary>
        /// Checking up transformability.
        /// </summary>
        public bool HasSupportingTransformer(Expression node)
        {
            return Transformers.Any(transformer => transformer.SupportTransform(node.NodeType));
        }

        /// <summary>
        /// Adding transfomer to context.
        /// </summary>
        public void AddTransfomer(IExpressionTransformer transfomer)
        {
            transformers.Add(transfomer);
        }
    }
}
