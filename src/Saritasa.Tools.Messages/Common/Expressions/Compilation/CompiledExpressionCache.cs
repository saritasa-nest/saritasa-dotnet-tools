using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    /// <summary>
    /// Cache of compiled expressions.
    /// </summary>
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
        public void Add(MethodInfo methodInfo, Func<Delegate> factory)
        {
            cache.GetOrAdd(methodInfo, (source) => factory());
        }

        /// <inheritdoc/>
        public bool HasKey(MethodInfo methodInfo)
        {
            return cache.ContainsKey(methodInfo);
        }
    }
}
