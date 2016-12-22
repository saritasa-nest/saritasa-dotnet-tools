using System;
using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    /// <summary>
    /// Contract for expression compilation.
    /// </summary>
    public interface IExpressionCompilator
    {
        /// <summary>
        /// Compiles and returning delegate.
        /// </summary>
        Delegate Compile(Expression input);
    }
}