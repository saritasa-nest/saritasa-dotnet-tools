using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public class ExpressionExecutor
    {
        private ExpressionExecutorContext context;

        public ExpressionExecutor(ExpressionExecutorContext context)
        {
            this.context = context;
        }

        public ExpressionExecutorContext Context => context;

        public Func<TInput, TResult> Compile<TInput, TResult>(Expression<Func<TInput, TResult>> expression)
        {
            return context.CompiledExpressionProvider.GetOrAdd<TInput, TResult>(GetMethodInfo(expression), () => (dynamic)context.Compilator.Compile(expression));
        }

        public TResult Execute<TInput, TResult>(Expression<Func<TInput, TResult>> expression, TInput input)
        {
            var compiledExpression = context.CompiledExpressionProvider.GetOrAdd<TInput, TResult>(GetMethodInfo(expression), () => (dynamic)context.Compilator.Compile(expression));

            return compiledExpression.Invoke(input);
        }

        private MethodInfo GetMethodInfo<TInput, TResult>(Expression<Func<TInput, TResult>> expression)
        {
            var body = expression.Body;
            if (body is MethodCallExpression)
            {
                return (body as MethodCallExpression).Method;
            }

            throw new NotSupportedException();
        }
    }
}
