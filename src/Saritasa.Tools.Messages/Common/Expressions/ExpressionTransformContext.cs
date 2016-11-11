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

        public ExpressionTransformContext(IList<IExpressionTransformer> transformers)
        {
            this.transformers = transformers;
        }

        /// <summary>
        /// Transformers of expression.
        /// </summary>
        public IReadOnlyCollection<IExpressionTransformer> Transformers => new ReadOnlyCollection<IExpressionTransformer>(transformers);

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
