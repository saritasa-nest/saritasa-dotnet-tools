using System;
using System.Linq.Expressions;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    public interface IExpressionCompilator
    {
        Func<TInput, TResult> Compile<TInput, TResult>(Expression<Func<TInput, TResult>> compilationExpression);

        Func<TInput, TInput2, TResult> Compile<TInput, TInput2, TResult>(Expression<Func<TInput, TInput2, TResult>> compilationExpression);
        
        Func<TInput, TInput2, TInput3, TResult> Compile<TInput, TInput2, TInput3, TResult>(Expression<Func<TInput, TInput2, TInput3, TResult>> compilationExpression);
    }
}