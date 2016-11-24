using Saritasa.Tools.Messages.Common.Expressions.Reduce;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    /// <summary>
    /// Factory for creating expression reducer, see <see cref="IExpressionReduceVisitor"/>.
    /// </summary>
    public class ExpressionReduceVisitorFactory : IExpressionReduceVisitorFactory
    {
        /// <summary>
        /// Factory method that returns implementation of <see cref=" IExpressionReduceVisitor"/>.
        /// </summary>
        public IExpressionReduceVisitor Create()
        {
            return new ExpressionReduceVisitor();
        }
    }
}
