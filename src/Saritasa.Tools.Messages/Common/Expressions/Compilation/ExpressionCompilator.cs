using System;
using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    /// <summary>
    /// Compilator of expression
    /// </summary>
    public class ExpressionCompilator : IExpressionCompilator
    {
        /// <inheritdoc/>
        public Delegate Compile(Expression input)
        {
            return (input as LambdaExpression).Compile();
        }
    }
}
