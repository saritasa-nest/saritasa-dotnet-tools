using Saritasa.Tools.Messages.Common.Expressions.Compilation;
using Saritasa.Tools.Messages.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public static class ExpressionExecutorContextExtension
    {
        public static void ConfigureTransformation(this ExpressionExecutorContext @this, Action<ExpressionTransformContext> transformContext)
        {
            transformContext(@this.TransformContext);

            @this.TransformerFactory = new ExpressionTransformVisitorFactory(@this.TransformContext);
        }

        public static void UseCompiledCache<T>(this ExpressionExecutorContext @this) where T : ICompiledExpressionProvider
        {
            @this.CompiledExpressionProvider = TypeHelpers.ResolveObjectForType(typeof(T), null) as ICompiledExpressionProvider;
        }

        public static void UseCompiledCache<T>(this ExpressionExecutorContext @this, Func<Type, object> compiledExpressionProviderResolver) where T : ICompiledExpressionProvider
        {
            @this.CompiledExpressionProvider = TypeHelpers.ResolveObjectForType(typeof(T), compiledExpressionProviderResolver) as ICompiledExpressionProvider;
        }
    }
}
