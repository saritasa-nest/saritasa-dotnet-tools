using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions.Reduce
{
    /// <summary>
    /// Used for reduce
    /// </summary>
    public interface IExpressionReduceVisitor
    {
        /// <summary>
        /// Visiting expression node and reduce outer scope values with binary operations by evaluating theirs.
        /// </summary>
        Expression VisitAndReduce(Expression node);
    }
}