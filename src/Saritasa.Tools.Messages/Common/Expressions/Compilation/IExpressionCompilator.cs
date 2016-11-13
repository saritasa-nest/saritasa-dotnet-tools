using System;
using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    public interface IExpressionCompilator
    {
        Delegate Compile(Expression input);
    }
}