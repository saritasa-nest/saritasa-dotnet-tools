using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public interface IExpressionReducer
    {
        Expression Reduce(Expression input);
    }
}