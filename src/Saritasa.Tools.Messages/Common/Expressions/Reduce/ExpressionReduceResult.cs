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
        /// Parameters after reduce, if we have const in expression query, we collapse theirs.
        /// </summary>
        public dynamic[] Parameters { get; set; }
    }
}
