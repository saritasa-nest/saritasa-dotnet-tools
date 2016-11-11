using Saritasa.Tools.Messages.Common.Expressions.Compilation;
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
        private ICompiledExpressionProvider expressionProvider;
        private IExpressionCompilator expressionCompilator;
        private IExpressionTransformVisitorFactory transformVisitorFactory;

        public ExpressionExecutor(
            ICompiledExpressionProvider expressionProvider,
            IExpressionCompilator expressionCompilator,
            IExpressionTransformVisitorFactory transformVisitorFactory)
        {
            this.expressionCompilator = expressionCompilator;
            this.expressionProvider = expressionProvider;
            this.transformVisitorFactory = transformVisitorFactory;
        }

        public TResult Execute<TInput, TResult>(Expression<Func<TInput, TResult>> expression, TInput input)
        {
            var compiledExpression = expressionProvider.GetOrAdd<TInput, TResult>(GetMethodInfo(expression), () =>
            {
                var transformer = transformVisitorFactory.Create();
                var transformedExpression = transformer.Visit(expression);
                return (dynamic)expressionCompilator.Compile((Expression<Func<TInput, TResult>>)transformedExpression);
            });

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
