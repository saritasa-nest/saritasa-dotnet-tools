using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    /// <summary>
    /// Transforming expression contract.
    /// </summary>
    public interface IExpressionTransformer
    {
        /// <summary>
        /// Checking up if transformer can transform expression
        /// </summary>
        /// <param name="nodeFrom">Single expression node which we will transform.</param>
        bool SupportTransform(ExpressionType nodeFrom);

        /// <summary>
        /// Transforming expression.
        /// </summary>
        Expression Transform(Expression input, ExpressionTransformContext context);
    }
}
