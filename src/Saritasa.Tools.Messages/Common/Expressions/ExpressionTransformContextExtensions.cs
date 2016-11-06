using Saritasa.Tools.Messages.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions
{
    public static class ExpressionTransformContextExtensions
    {
        public static void UseTransfomer<T>(this ExpressionTransformContext @this) where T : IExpressionTransformer
        {
            UseTransfomer<T>(@this, null);
        }

        public static void UseTransfomer<T>(this ExpressionTransformContext @this, Func<Type, object> resolver) where T : IExpressionTransformer
        {
            var transfomer = TypeHelpers.ResolveObjectForType(typeof(T), resolver) as IExpressionTransformer;
            @this.AddTransfomer(transfomer);
        }
    }
}
