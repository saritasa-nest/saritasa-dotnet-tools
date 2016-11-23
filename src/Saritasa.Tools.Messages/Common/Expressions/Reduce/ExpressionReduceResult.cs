using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions.Reduce
{
    /// <summary>
    /// Data object representing reduce result.
    /// </summary>
    public class ExpressionReduceResult
    {
        /// <summary>
        /// Expression after reduce.
        /// </summary>
        public Expression ReducedExpression { get; set; }

        /// <summary>
        /// Reduced parameters on method call arguments if we have reduced they.
        /// </summary>
        public dynamic[] Parameters { get; set; }
    }
}
