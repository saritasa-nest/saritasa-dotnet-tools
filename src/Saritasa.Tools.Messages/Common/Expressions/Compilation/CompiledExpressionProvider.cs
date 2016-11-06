using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    public class CompiledExpressionProvider : ICompiledExpressionProvider
    {
        private ConcurrentDictionary<MethodInfo, dynamic> cache = new ConcurrentDictionary<MethodInfo, dynamic>();

        public IReadOnlyDictionary<MethodInfo, dynamic> KeyValues => cache;

        public Func<TInput, TResult> GetOrAdd<TInput, TResult>(MethodInfo methodInfo, Func<dynamic> factory)
        {
            return cache.GetOrAdd(methodInfo, (source) => factory());
        }
    }
}
