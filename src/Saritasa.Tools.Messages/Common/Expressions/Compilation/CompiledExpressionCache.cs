using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace Saritasa.Tools.Messages.Common.Expressions.Compilation
{
    /// <summary>
    /// Cache of compiled expressions.
    /// </summary>
    public class CompiledExpressionCache : ICompiledExpressionCache
    {
        private static ConcurrentDictionary<MethodInfo, Delegate> cache = new ConcurrentDictionary<MethodInfo, Delegate>();
        private static ReaderWriterLockSlim @lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        /// <inheritdoc />
        public int Count
        {
            get
            {
                @lock.EnterReadLock();
                try
                {
                    return cache.Count;
                }
                finally
                {
                    @lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            @lock.EnterWriteLock();
            try
            {
                cache.Clear();
            }
            finally
            {
                @lock.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public Delegate Get(MethodInfo methodInfo)
        {
            @lock.EnterReadLock();
            try
            {
                if (!cache.ContainsKey(methodInfo))
                {
                    throw new KeyNotFoundException("Failed to find provided key in cache.");
                }

                return cache[methodInfo];
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        public Delegate GetOrAdd(MethodInfo methodInfo, Func<Delegate> factory)
        {
            @lock.EnterUpgradeableReadLock();
            try
            {
                return cache.GetOrAdd(methodInfo, (source) =>
                {
                    @lock.EnterWriteLock();
                    try
                    {
                        return factory();
                    }
                    finally
                    {
                        @lock.ExitWriteLock();
                    }
                });
            }
            finally
            {
                @lock.ExitUpgradeableReadLock();
            }
        }

        /// <inheritdoc/>
        public bool HasKey(MethodInfo methodInfo)
        {
            @lock.EnterReadLock();
            try
            {
                return cache.ContainsKey(methodInfo);
            }
            finally
            {
                @lock.ExitReadLock();
            }
        }
    }
}
