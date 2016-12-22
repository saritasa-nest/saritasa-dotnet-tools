using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public interface IExpressionTransformVisitorFactory
    {
        ExpressionVisitor Create();
    }
}
