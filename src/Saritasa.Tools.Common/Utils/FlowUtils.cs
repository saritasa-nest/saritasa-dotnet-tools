// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
#if PORTABLE || NETSTANDARD1_6 || NETCOREAPP1_0
    using System.Reflection;
#endif

    /// <summary>
    /// Provides methods to control execution flow.
    /// </summary>
    public static class FlowUtils
    {
        /// <summary>
        /// Retry strategy delegate determines wait delay and when to break attempts to execute.
        /// </summary>
        /// <param name="attemptCount">Current attempt number.</param>
        /// <param name="lastException">Last exception</param>
        /// <param name="neededDelay">Out argument determines how much time to delay.</param>
        /// <returns>Return true if need to break flow.</returns>
        public delegate bool RetryStrategy(int attemptCount, Exception lastException, out TimeSpan neededDelay);

        /// <summary>
        /// Retry callback that can be used for logging or debug purposes.
        /// </summary>
        /// <param name="attemptCount">Current attempt number.</param>
        /// <param name="lastException">Last exception.</param>
        public delegate void RetryCallback(int attemptCount, Exception lastException);

        /// <summary>
        /// Every call of action retries up to numberOfTries times if any subclass of exceptions
        /// occures.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="retryStrategy">Retry strategy to use.</param>
        /// <param name="transientExceptions">Set of exceptions on which repeat occures. If null retry will appear on any exception.</param>
        /// <returns>Specified user type.</returns>
        public static T Retry<T>(Func<T> action, RetryStrategy retryStrategy, params Type[] transientExceptions)
        {
            Guard.IsNotNull(action, nameof(action));
            Guard.IsNotNull(retryStrategy, nameof(retryStrategy));

            int attemptCount = 0;
            while (true)
            {
                attemptCount++;
                try
                {
                    return action();
                }
                catch (Exception executedException)
                {
                    bool isTransient = IsSubtypeOf(executedException, transientExceptions);
                    if (!isTransient)
                    {
                        throw;
                    }

                    TimeSpan delay;
                    bool shouldStop = retryStrategy(attemptCount, executedException, out delay);
                    if (shouldStop)
                    {
                        throw;
                    }
                    if (delay.TotalMilliseconds > 0)
                    {
#if PORTABLE || NETCOREAPP1_0 || NETSTANDARD1_6
                        System.Threading.Tasks.Task.Delay(delay).Wait();
#else
                        Thread.Sleep((int)delay.TotalMilliseconds);
#endif
                    }
                }
            }
        }

        /// <summary>
        /// Every call of action retries up to numberOfTries times if any subclass of exceptions
        /// occures.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="retryStrategy">Retry strategy.</param>
        /// <param name="transientExceptions">Set of exceptions on which repeat occures. If null retry will appear on any exception.</param>
        public static void Retry(Action action, RetryStrategy retryStrategy, params Type[] transientExceptions)
        {
            FlowUtils.Retry(
                () =>
                {
                    action();
                    return true;
                },
                retryStrategy,
                transientExceptions
            );
        }

        /// <summary>
        /// Provides the async implementation of the retry mechanism for unreliable actions and transient conditions.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="action">Unreliable action to execute.</param>
        /// <param name="retryStrategy">Retry strategy.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="transientExceptions">Transient exceptions.</param>
        /// <returns>Task that specified when action executed successfully or with error after all retries.</returns>
        public static async Task<T> RetryAsync<T>(
            Func<Task<T>> action,
            RetryStrategy retryStrategy,
            CancellationToken cancellationToken = default(CancellationToken),
            params Type[] transientExceptions)
        {
            Guard.IsNotNull(action, nameof(action));
            Guard.IsNotNull(retryStrategy, nameof(retryStrategy));

            // based on TPL police we should check whether action already cancelled
            if (cancellationToken.IsCancellationRequested)
            {
#if NET46
                return await Task.FromCanceled<T>(cancellationToken);
#else
                var tcs = new TaskCompletionSource<T>();
                tcs.SetCanceled();
                return await tcs.Task.ConfigureAwait(false);
#endif
            }

            int attemptCount = 0;
            while (true)
            {
                attemptCount++;
                try
                {
                    return await action().ConfigureAwait(false);
                }
                catch (Exception executedException)
                {
                    bool isTransient = IsSubtypeOf(executedException, transientExceptions);
                    if (!isTransient)
                    {
                        throw;
                    }

                    TimeSpan delay;
                    bool shouldStop = retryStrategy(attemptCount, executedException, out delay);
                    if (shouldStop)
                    {
                        throw;
                    }
                    if (delay.TotalMilliseconds > 0)
                    {
                        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if executedException is type of subtype one of exceptionsTypes.
        /// </summary>
        /// <param name="executedException">Exception to check.</param>
        /// <param name="exceptionsTypes">Exceptions of check.</param>
        internal static bool IsSubtypeOf(Exception executedException, Type[] exceptionsTypes)
        {
            if (exceptionsTypes == null || exceptionsTypes.Length < 1)
            {
                return true;
            }

            bool isSubclass = false;
            Type executedExceptionType = executedException.GetType();
            foreach (var exceptionType in exceptionsTypes)
            {
#if PORTABLE || NETSTANDARD1_6 || NETCOREAPP1_0
                if (executedExceptionType.Equals(exceptionType) || executedExceptionType.GetTypeInfo().IsSubclassOf(exceptionType))
#else
                if (executedExceptionType == exceptionType || executedExceptionType.IsSubclassOf(exceptionType))
#endif
                {
                    isSubclass = true;
                    break;
                }
            }
            return isSubclass;
        }

        #region Retry strategies

        /// <summary>
        /// Creates delegate with fixed wait time between calls.
        /// </summary>
        /// <param name="numberOfTries">Maximum number of tries, default is 3.</param>
        /// <param name="delay">Delay between calls. Default is zero.</param>
        /// <param name="firstFastRetry">Do not delay for second attempt.</param>
        /// <returns>Retry strategy delegate.</returns>
        public static RetryStrategy CreateFixedDelayRetryStrategy(int numberOfTries = 3, TimeSpan? delay = null, bool firstFastRetry = false)
        {
            if (delay == null)
            {
                delay = TimeSpan.Zero;
            }
            Guard.IsNotNegativeOrZero(numberOfTries, nameof(numberOfTries));
            Guard.IsNotNegative(delay.Value, nameof(delay));

            return (int attemptCount, Exception lastException, out TimeSpan neededDelay) =>
            {
                if (attemptCount >= numberOfTries)
                {
                    neededDelay = TimeSpan.Zero;
                    return true;
                }

                neededDelay = attemptCount == 1 && firstFastRetry ? TimeSpan.Zero : delay.Value;
                return false;
            };
        }

        /// <summary>
        /// Creates incremental wait time retry strategy. Wait time is incremented by "increment" time.
        /// </summary>
        /// <param name="numberOfTries">Maximum number of tries, default is 3.</param>
        /// <param name="delay">Delay between calls. Default is zero.</param>
        /// <param name="increment">Increment time with every call. Default is 0.</param>
        /// <param name="firstFastRetry">Do not delay for second attempt.</param>
        /// <returns>Retry strategy delegate.</returns>
        public static RetryStrategy CreateIncrementDelayRetryStrategy(
            int numberOfTries = 3,
            TimeSpan? delay = null,
            TimeSpan? increment = null,
            bool firstFastRetry = false)
        {
            if (delay == null)
            {
                delay = TimeSpan.Zero;
            }
            if (increment == null)
            {
                increment = TimeSpan.Zero;
            }
            Guard.IsNotNegativeOrZero(numberOfTries, nameof(numberOfTries));
            Guard.IsNotNegative(delay.Value, nameof(delay));
            Guard.IsNotNegative(increment.Value, nameof(increment));

            return (int attemptCount, Exception lastException, out TimeSpan neededDelay) =>
            {
                if (attemptCount >= numberOfTries)
                {
                    neededDelay = TimeSpan.Zero;
                    return true;
                }

                neededDelay = attemptCount == 1 && firstFastRetry ?
                    TimeSpan.Zero :
                    TimeSpan.FromMilliseconds(delay.Value.TotalMilliseconds + (attemptCount * increment.Value.TotalMilliseconds));
                return false;
            };
        }

        /// <summary>
        /// A retry strategy with backoff parameters for calculating the exponential delay between retries.
        /// </summary>
        /// <param name="numberOfTries">Maximum number of tries, default is 3.</param>
        /// <param name="minBackoff">The minimum backoff time.</param>
        /// <param name="maxBackoff">The maximum backoff time</param>
        /// <param name="deltaBackoff">The value that will be used to calculate a random delta in the exponential delay between retries.</param>
        /// <param name="firstFastRetry">Do not delay for second attempt.</param>
        /// <returns>Retry strategy delegate.</returns>
        public static RetryStrategy CreateExponentialBackoffDelayRetryStrategy(
            int numberOfTries = 3,
            TimeSpan? minBackoff = null,
            TimeSpan? maxBackoff = null,
            TimeSpan? deltaBackoff = null,
            bool firstFastRetry = false)
        {
            if (minBackoff == null)
            {
                minBackoff = TimeSpan.FromSeconds(1.0);
            }
            if (maxBackoff == null)
            {
                maxBackoff = TimeSpan.FromSeconds(30.0);
            }
            if (deltaBackoff == null)
            {
                deltaBackoff = TimeSpan.FromSeconds(10.0);
            }
            Guard.IsNotNegativeOrZero(numberOfTries, nameof(numberOfTries));
            Guard.IsNotNegative(minBackoff.Value, nameof(minBackoff));
            Guard.IsNotNegative(maxBackoff.Value, nameof(maxBackoff));
            Guard.IsNotNegative(deltaBackoff.Value, nameof(deltaBackoff));
            if (minBackoff > maxBackoff)
            {
                throw new ArgumentOutOfRangeException(nameof(minBackoff), "minBackoff cannot be less than maxBackoff");
            }

            return (int attemptCount, Exception lastException, out TimeSpan neededDelay) =>
            {
                if (attemptCount >= numberOfTries)
                {
                    neededDelay = TimeSpan.Zero;
                    return true;
                }

                if (attemptCount == 1 && firstFastRetry)
                {
                    neededDelay = TimeSpan.Zero;
                }
                else
                {
                    Random random = new Random();
                    int num = (int)((Math.Pow(2.0, attemptCount) - 1.0) *
                                    random.Next((int)(deltaBackoff.Value.TotalMilliseconds * 0.8), (int)(deltaBackoff.Value.TotalMilliseconds * 1.2)));
                    int num2 = (int)Math.Min(minBackoff.Value.TotalMilliseconds + num, maxBackoff.Value.TotalMilliseconds);
                    neededDelay = TimeSpan.FromMilliseconds(num2);
                }
                return false;
            };
        }

        /// <summary>
        /// Creates wrapper delegate around "RetryCallback". Can be used for loggin or debug purpose.
        /// Please note that this delegate should be passed first when combine with RetryStrategyDelegate.
        /// </summary>
        /// <param name="callback">User callback.</param>
        /// <returns>Retry strategy delegate.</returns>
        public static RetryStrategy CreateCallbackRetryStrategy(RetryCallback callback)
        {
            return (int attemptCount, Exception lastException, out TimeSpan neededDelay) =>
            {
                neededDelay = TimeSpan.Zero;
                try
                {
                    callback(attemptCount, lastException);
                }
                catch (Exception)
                {
                    // ignored
                }
                return false;
            };
        }

        #endregion

        /// <summary>
        /// It is null-safe and thread-safe way to raise event.
        /// </summary>
        public static void Raise<TEventArgs>(object sender, TEventArgs e, ref EventHandler<TEventArgs> eventDelegate)
        {
            var temp = Volatile.Read(ref eventDelegate);
#if !PORTABLE && !NETSTANDARD1_6 && !NETCOREAPP1_0
            Thread.MemoryBarrier();
#endif
            temp?.Invoke(sender, e);
        }

        /// <summary>
        /// It is null-safe and thread-safe way to raise event. The method calls every handler related to event.
        /// If any handler throws an error the AggregateException will be thrown.
        /// </summary>
        public static void RaiseAll<TEventArgs>(object sender, TEventArgs e, ref EventHandler<TEventArgs> eventDelegate)
        {
            var temp = Volatile.Read(ref eventDelegate);
#if !PORTABLE && !NETSTANDARD1_6 && !NETCOREAPP1_0
            Thread.MemoryBarrier();
#endif
            if (temp == null)
            {
                return;
            }

            var exceptions = new List<Exception>();
            foreach (var handler in temp.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(sender, e);
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    exceptions.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// Throw the exception to skip item memoization.
        /// </summary>
#if !PORTABLE && !NETSTANDARD1_6 && !NETCOREAPP1_0
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly", Justification = "GetObjectData is not needed")]
        [Serializable]
#endif
        public sealed class SkipMemoizeException<TResult> : Exception
        {
            private readonly TResult result;

            /// <summary>
            /// Returned result.
            /// </summary>
            public TResult Result => result;

            /// <summary>
            /// .ctor
            /// </summary>
            /// <param name="result">Result from memoize delegate.</param>
            public SkipMemoizeException(TResult result)
            {
                this.result = result;
            }
        }

        /// <summary>
        /// Cache strategy delegate determines when value must be invalidated.
        /// </summary>
        /// <typeparam name="TKey">Cache function key type.</typeparam>
        /// <typeparam name="TResult">Cache function result type.</typeparam>
        /// <param name="key">Cache key.</param>
        /// <param name="cache">Cache storage used.</param>
        /// <param name="notInCache">The key was not in cache.</param>
        /// <returns>The value must be evaluated and cached again.</returns>
        public delegate bool CacheStrategy<TKey, TResult>(TKey key, IDictionary<TKey, TResult> cache, bool notInCache);

        /// <summary>
        /// Cache strategy delegate determines when value must be invalidated without key.
        /// Allows to use with delegates with no arguments.
        /// </summary>
        /// <typeparam name="TResult">Cache function result type.</typeparam>
        /// <param name="cache">Cache storage used.</param>
        /// <param name="notInCache">The key was not in cache.</param>
        /// <returns>The value must be evaluated and cached again.</returns>
        public delegate bool CacheStrategy<TResult>(IDictionary<int, TResult> cache, bool notInCache);

        #region MaxAgeCacheStrategy

        /// <summary>
        /// Cache strategy based on age validation. If item exceed specific time on life it shoule be
        /// invalidated.
        /// </summary>
        /// <typeparam name="TKey">Cache key type.</typeparam>
        /// <typeparam name="TResult">Cache function result type.</typeparam>
        /// <param name="maxAge">Maximum age to live.</param>
        /// <param name="timestampsStorage">Storage to be used for timestamps. By default Dictionary is used.</param>
        /// <returns>Cache strategy instance delegate.</returns>
        public static CacheStrategy<TKey, TResult> CreateMaxAgeCacheStrategy<TKey, TResult>(
            TimeSpan maxAge,
            IDictionary<TKey, DateTime> timestampsStorage = null)
        {
            if (timestampsStorage == null)
            {
                timestampsStorage = new Dictionary<TKey, DateTime>();
            }
            object lockobj = new object();

            return (key, dict, notInCache) =>
            {
                DateTime dt;
                bool cached;
                lock (lockobj)
                {
                    cached = timestampsStorage.TryGetValue(key, out dt);
                    if (!cached)
                    {
                        timestampsStorage[key] = DateTime.Now;
                    }
                }
                return !cached || (DateTime.Now - dt) >= maxAge;
            };
        }

        /// <summary>
        /// Cache strategy based on age validation. If item exceed specific time on life it shoule be
        /// invalidated. Overload for delegates with no arguments.
        /// </summary>
        /// <typeparam name="TResult">Cache function result type.</typeparam>
        /// <param name="maxAge">Maximum age to live.</param>
        /// <param name="timestampsStorage"></param>
        /// <returns>Cache strategy instance delegate.</returns>
        public static CacheStrategy<int, TResult> CreateMaxAgeCacheStrategy<TResult>(
            TimeSpan maxAge,
            IDictionary<int, DateTime> timestampsStorage = null)
        {
            return CreateMaxAgeCacheStrategy<int, TResult>(maxAge, timestampsStorage);
        }

        /// <summary>
        /// Cache strategy based on age validation. If item exceed specific time on life it shoule be
        /// invalidated. Overload for delegates with 2 arguments.
        /// </summary>
        /// <typeparam name="T1">Type component for key.</typeparam>
        /// <typeparam name="T2">Type component for key.</typeparam>
        /// <typeparam name="TResult">Cache function result type.</typeparam>
        /// <param name="maxAge">Maximum age to live.</param>
        /// <returns>Cache strategy instance delegate.</returns>
        public static CacheStrategy<Tuple<T1, T2>, TResult> CreateMaxAgeCacheStrategy<T1, T2, TResult>(TimeSpan maxAge)
        {
            return CreateMaxAgeCacheStrategy<Tuple<T1, T2>, TResult>(maxAge);
        }

        /// <summary>
        /// Cache strategy based on age validation. If item exceed specific time on life it shoule be
        /// invalidated. Overload for delegates with 3 arguments.
        /// </summary>
        /// <typeparam name="T1">Type component for key.</typeparam>
        /// <typeparam name="T2">Type component for key.</typeparam>
        /// <typeparam name="T3">Type component for key.</typeparam>
        /// <typeparam name="TResult">Cache function result type.</typeparam>
        /// <param name="maxAge">Maximum age to live.</param>
        /// <returns>Cache strategy instance delegate.</returns>
        public static CacheStrategy<Tuple<T1, T2, T3>, TResult> CreateMaxAgeCacheStrategy<T1, T2, T3, TResult>(TimeSpan maxAge)
        {
            return CreateMaxAgeCacheStrategy<Tuple<T1, T2, T3>, TResult>(maxAge);
        }

        #endregion

        #region MaxCountCacheStrategy

        /// <summary>
        /// Cache strategy invalidation based on max count if items cached. If it exceeds maxCount the
        /// removeCount last items will be removed. If purge is true then whole cached will be cleared.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TResult">Cache function result type.</typeparam>
        /// <param name="maxCount">Max items to cache.</param>
        /// <param name="removeCount">How many items to remove from cache, default is 1.</param>
        /// <param name="purge">Should whole cache be cleared. If true removeCount parameter is ignored. False by default.</param>
        /// <param name="keysStorage">Storage for keys.</param>
        /// <returns>Cache strategy instance delegate.</returns>
        public static CacheStrategy<TKey, TResult> CreateMaxCountCacheStrategy<TKey, TResult>(
            int maxCount,
            int removeCount = 1,
            bool purge = false,
            IList<TKey> keysStorage = null)
        {
            if (maxCount < removeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount), "maxCount cannot be less than removeCount");
            }
            if (keysStorage == null && !purge)
            {
                keysStorage = new List<TKey>();
            }
            object lockobj = new object();

            return (TKey key, IDictionary<TKey, TResult> dict, bool notInCache) =>
            {
                if (notInCache && !purge)
                {
                    lock (lockobj)
                    {
                        keysStorage.Add(key);
                    }
                }
                if (dict.Count > maxCount && keysStorage.Count > 0)
                {
                    if (purge)
                    {
                        lock (lockobj)
                        {
                            keysStorage.Clear();
                        }
                        dict.Clear();
                    }
                    else
                    {
                        var toRemove = new TKey[removeCount];
                        lock (lockobj)
                        {
                            for (int i = 0; i < removeCount && keysStorage.Count > 0; i++)
                            {
                                var item = keysStorage[0];
                                keysStorage.RemoveAt(0);
                                toRemove[i] = item;
                            }
                        }
                        for (int i = 0; i < toRemove.Length; i++)
                        {
                            dict.Remove(toRemove[i]);
                        }
                    }
                }
                return false;
            };
        }

        /// <summary>
        /// Cache strategy invalidation based on max count if items cached. If it exceeds maxCount the
        /// removeCount last items will be removed. If purge is true then whole cached will be cleared.
        /// Overload for delegates with no arguments.
        /// </summary>
        /// <typeparam name="TResult">Cache function result type.</typeparam>
        /// <param name="maxCount">Max items to cache.</param>
        /// <param name="removeCount">How many items to remove from cache, default is 1.</param>
        /// <param name="purge">Should whole cache be cleared. If true removeCount parameter is ignored. False by default.</param>
        /// <param name="keysStorage">Storage for keys.</param>
        /// <returns>Cache strategy instance delegate.</returns>
        public static CacheStrategy<int, TResult> CreateMaxCountCacheStrategy<TResult>(
            int maxCount,
            int removeCount = 1,
            bool purge = false,
            IList<int> keysStorage = null)
        {
            return CreateMaxCountCacheStrategy<int, TResult>(maxCount, removeCount, purge, keysStorage);
        }

        /// <summary>
        /// Cache strategy invalidation based on max count if items cached. If it exceeds maxCount the
        /// removeCount last items will be removed. If purge is true then whole cached will be cleared.
        /// Overload for delegates with 2 arguments.
        /// </summary>
        /// <typeparam name="T1">Type component for key.</typeparam>
        /// <typeparam name="T2">Type component for key.</typeparam>
        /// <typeparam name="TResult">Cache function result type.</typeparam>
        /// <param name="maxCount">Max items to cache.</param>
        /// <param name="removeCount">How many items to remove from cache, default is 1.</param>
        /// <param name="purge">Should whole cache be cleared. If true removeCount parameter is ignored. False by default.</param>
        /// <param name="keysStorage">Storage for keys.</param>
        /// <returns>Cache strategy instance delegate.</returns>
        public static CacheStrategy<Tuple<T1, T2>, TResult> CreateMaxCountCacheStrategy<T1, T2, TResult>(
            int maxCount,
            int removeCount = 1,
            bool purge = false,
            IList<Tuple<T1, T2>> keysStorage = null)
        {
            return CreateMaxCountCacheStrategy<Tuple<T1, T2>, TResult>(maxCount, removeCount, purge, keysStorage);
        }

        /// <summary>
        /// Cache strategy invalidation based on max count if items cached. If it exceeds maxCount the
        /// removeCount last items will be removed. If purge is true then whole cached will be cleared.
        /// Overload for delegates with 3 arguments.
        /// </summary>
        /// <typeparam name="T1">Type component for key.</typeparam>
        /// <typeparam name="T2">Type component for key.</typeparam>
        /// <typeparam name="T3">Type component for key.</typeparam>
        /// <typeparam name="TResult">Cache function result type.</typeparam>
        /// <param name="maxCount">Max items to cache.</param>
        /// <param name="removeCount">How many items to remove from cache, default is 1.</param>
        /// <param name="purge">Should whole cache be cleared. If true removeCount parameter is ignored. False by default.</param>
        /// <param name="keysStorage">Storage for keys.</param>
        /// <returns>Cache strategy instance delegate.</returns>
        public static CacheStrategy<Tuple<T1, T2, T3>, TResult> CreateMaxCountCacheStrategy<T1, T2, T3, TResult>(
            int maxCount,
            int removeCount = 1,
            bool purge = false,
            IList<Tuple<T1, T2, T3>> keysStorage = null)
        {
            return CreateMaxCountCacheStrategy<Tuple<T1, T2, T3>, TResult>(maxCount, removeCount, purge, keysStorage);
        }

        #endregion

        /// <summary>
        /// Returns a memoized version of a referentially transparent function. The memoized version of the
        /// function keeps a cache of the mapping from arguments to results and, when calls with the same
        /// arguments are repeated often, has higher performance at the expense of higher memory use.
        /// </summary>
        /// <typeparam name="TKey">First argument type.</typeparam>
        /// <typeparam name="TResult">Function result type.</typeparam>
        /// <param name="func">The function to memoize.</param>
        /// <param name="strategies">Strategies to apply. By default limitless strategy will be used.</param>
        /// <param name="cache">Dictionary to use for caching. If not specified the standard Dictionary will be used which
        /// is not thread safe.</param>
        /// <returns>Delegate the able to cache.</returns>
        public static Func<TKey, TResult> Memoize<TKey, TResult>(
            Func<TKey, TResult> func,
            CacheStrategy<TKey, TResult> strategies = null,
            IDictionary<TKey, TResult> cache = null)
        {
            if (cache == null)
            {
                cache = new Dictionary<TKey, TResult>();
            }
            if (strategies == null)
            {
                strategies = (key, dict, notInCache) => false;
            }

            return (key) =>
            {
                TResult result = default(TResult);
                bool needUpdate = false, strategiesAlreadyApplied = false;

                // if result is already in cache and no need to refresh it just skip
                bool inCache = cache.TryGetValue(key, out result);

                if (inCache)
                {
                    // we may combine strategies
                    foreach (CacheStrategy<TKey, TResult> strategy in strategies.GetInvocationList())
                    {
                        // we have to go thru whole list because some strategies may refresh cache
                        bool ret = strategy(key, cache, false);
                        if (ret)
                        {
                            needUpdate = ret;
                        }
                    }
                    if (!needUpdate)
                    {
                        return result;
                    }
                    strategiesAlreadyApplied = true;
                }

                // call user func
                try
                {
                    result = func(key);
                    cache[key] = result;
                }
                catch (SkipMemoizeException<TResult> exc)
                {
                    strategiesAlreadyApplied = true;
                    result = exc.Result;
                }

                // if we didn't call strategies yet
                if (!strategiesAlreadyApplied)
                {
                    foreach (CacheStrategy<TKey, TResult> strategy in strategies.GetInvocationList())
                    {
                        // we have to go thru whole list because some strategies may refresh cache
                        bool ret = strategy(key, cache, true);
                        if (ret)
                        {
                            needUpdate = ret;
                        }
                    }
                }
                return result;
            };
        }

        /// <summary>
        /// Returns a memoized version of a referentially transparent function. The memoized version of the
        /// function keeps a cache of the mapping from arguments to results and, when calls with the same
        /// arguments are repeated often, has higher performance at the expense of higher memory use.
        /// Overload for delegates with no arguments.
        /// </summary>
        /// <typeparam name="TResult">Function result type.</typeparam>
        /// <param name="func">The function to memoize.</param>
        /// <param name="strategies">Strategies to apply. By default limitless strategy will be used.</param>
        /// <param name="cache">Dictionary to use for caching. If not specified the standard Dictionary will be used which
        /// is not thread safe.</param>
        /// <returns>Delegate the able to cache.</returns>
        public static Func<TResult> Memoize<TResult>(
            Func<TResult> func,
            CacheStrategy<int, TResult> strategies = null,
            IDictionary<int, TResult> cache = null)
        {
            var func2 = new Func<int, TResult>((arg) => func());
            var memorized = Memoize(func2, strategies, cache);
            return () => memorized(0);
        }

        /// <summary>
        /// Returns a memoized version of a referentially transparent function. The memoized version of the
        /// function keeps a cache of the mapping from arguments to results and, when calls with the same
        /// arguments are repeated often, has higher performance at the expense of higher memory use.
        /// Overload for delegates with 2 arguments.
        /// </summary>
        /// <typeparam name="T1">Type component for key.</typeparam>
        /// <typeparam name="T2">Type component for key.</typeparam>
        /// <typeparam name="TResult">Function result type.</typeparam>
        /// <param name="func">The function to memoize.</param>
        /// <param name="strategies">Strategies to apply. By default limitless strategy will be used.</param>
        /// <param name="cache">Dictionary to use for caching. If not specified the standard Dictionary will be used which
        /// is not thread safe.</param>
        /// <returns>Delegate the able to cache.</returns>
        public static Func<T1, T2, TResult> Memoize<T1, T2, TResult>(
            Func<T1, T2, TResult> func,
            CacheStrategy<Tuple<T1, T2>, TResult> strategies = null,
            IDictionary<Tuple<T1, T2>, TResult> cache = null)
        {
            return Memoize(func, strategies, cache);
        }

        /// <summary>
        /// Returns a memoized version of a referentially transparent function. The memoized version of the
        /// function keeps a cache of the mapping from arguments to results and, when calls with the same
        /// arguments are repeated often, has higher performance at the expense of higher memory use.
        /// Overload for delegates with 3 arguments.
        /// </summary>
        /// <typeparam name="T1">Type component for key.</typeparam>
        /// <typeparam name="T2">Type component for key.</typeparam>
        /// <typeparam name="T3">Type component for key.</typeparam>
        /// <typeparam name="TResult">Function result type.</typeparam>
        /// <param name="func">The function to memoize.</param>
        /// <param name="strategies">Strategies to apply. By default limitless strategy will be used.</param>
        /// <param name="cache">Dictionary to use for caching. If not specified the standard Dictionary will be used which
        /// is not thread safe.</param>
        /// <returns>Delegate the able to cache.</returns>
        public static Func<T1, T2, T3, TResult> Memoize<T1, T2, T3, TResult>(
            Func<T1, T2, T3, TResult> func,
            CacheStrategy<Tuple<T1, T2, T3>, TResult> strategies = null,
            IDictionary<Tuple<T1, T2>, TResult> cache = null)
        {
            return Memoize(func, strategies, cache);
        }
    }
}
