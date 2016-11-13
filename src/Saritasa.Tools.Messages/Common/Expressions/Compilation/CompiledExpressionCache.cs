using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    public class CompiledExpressionCache : ICompiledExpressionCache
    {
        private static ConcurrentDictionary<MethodInfo, Delegate> cache = new ConcurrentDictionary<MethodInfo, Delegate>();

        /// <inheritdoc />
        public int Count => cache.Count;

        /// <inheritdoc />
        public void Clear()
        {
            cache.Clear();
        }

        /// <inheritdoc />
        public Delegate Get(MethodInfo methodInfo)
        {
            if (!cache.ContainsKey(methodInfo))
            {
                throw new KeyNotFoundException("Failed to find provided key in cache.");
            }

            return cache[methodInfo];
        }

        /// <inheritdoc />
        public Delegate GetOrAdd(MethodInfo methodInfo, Func<Delegate> factory)
        {
            return cache.GetOrAdd(methodInfo, (source) => factory());
        }
    }
}
