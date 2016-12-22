using System.Linq.Expressions;

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
        Expression Transform(Expression input, ExpressionTransformVisitor visitor);
    }
}
