using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    public class ExpressionCompilator : IExpressionCompilator
    {
        public Func<TInput, TResult> Compile<TInput, TResult>(Expression<Func<TInput, TResult>> compilationExpression)
        {
            return compilationExpression.Compile();
        }

        public Func<TInput, TInput2, TResult> Compile<TInput, TInput2, TResult>(Expression<Func<TInput, TInput2, TResult>> compilationExpression)
        {
            return compilationExpression.Compile();
        }

        public Func<TInput, TInput2, TInput3, TResult> Compile<TInput, TInput2, TInput3, TResult>(Expression<Func<TInput, TInput2,TInput3, TResult>> compilationExpression)
        {
            return compilationExpression.Compile();
        }
    }
}
