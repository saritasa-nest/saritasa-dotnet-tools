// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
#if NETSTANDARD1_2 || NETSTANDARD1_6 || NETSTANDARD2_0
using System.Reflection;
#endif

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Provides methods to control execution flow.
    /// </summary>
    public static partial class FlowUtils
    {
        /// <summary>
        /// Retry strategy delegate determines wait delay and when to break attempts to execute.
        /// </summary>
        /// <param name="attemptCount">Current attempt number.</param>
        /// <param name="lastException">Last exception.</param>
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
        /// occurs.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="retryStrategy">Retry strategy to use.</param>
        /// <param name="transientExceptions">SetPart of exceptions on which repeat occurs. If null retry will appear on any exception.</param>
        /// <returns>Specified user type.</returns>
        public static T Retry<T>(
            Func<T> action,
            RetryStrategy retryStrategy,
            params Type[] transientExceptions)
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
#if NETSTANDARD1_2 || NETSTANDARD1_6 || NETSTANDARD2_0
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
        /// occurs.
        /// </summary>
        /// <param name="action">Action to execute.</param>
        /// <param name="retryStrategy">Retry strategy.</param>
        /// <param name="transientExceptions">SetPart of exceptions on which repeat occurs. If null retry will appear on any exception.</param>
        public static void Retry(
            Action action,
            RetryStrategy retryStrategy,
            params Type[] transientExceptions)
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

#if NET40
        /// <summary>
        /// Provides the async implementation of the retry mechanism for unreliable actions and transient conditions.
        /// </summary>
        /// <typeparam name="T">Result type.</typeparam>
        /// <param name="action">Unreliable action to execute.</param>
        /// <param name="retryStrategy">Retry strategy.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="transientExceptions">Transient exceptions.</param>
        /// <returns>Task that specified when action executed successfully or with error after all retries.</returns>
        public static Task<T> RetryAsync<T>(
            Func<Task<T>> action,
            RetryStrategy retryStrategy,
            CancellationToken cancellationToken = default(CancellationToken),
            params Type[] transientExceptions)
        {
            Guard.IsNotNull(action, nameof(action));
            Guard.IsNotNull(retryStrategy, nameof(retryStrategy));
            return RetryAsyncInternal(action, 1, retryStrategy, cancellationToken, transientExceptions);
        }

        internal static Task<T> RetryAsyncInternal<T>(
            Func<Task<T>> action,
            int attemptCount,
            RetryStrategy retryStrategy,
            CancellationToken cancellationToken = default(CancellationToken),
            params Type[] transientExceptions)
        {
            // Based on TPL police we should check whether action already cancelled.
            if (cancellationToken.IsCancellationRequested)
            {
                var tcs1 = new TaskCompletionSource<T>();
                tcs1.SetCanceled();
                return tcs1.Task;
            }

            Task<T> task = null;
            try
            {
                task = action();
            }
            catch (Exception executedException)
            {
                // Check sync call, if exception occurs before task creation.
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
                    Thread.Sleep((int)delay.TotalMilliseconds);
                }

                var tcs2 = new TaskCompletionSource<T>();
                tcs2.SetException(executedException);
                task = tcs2.Task;
            }

            // Success case.
            if (task.Status == TaskStatus.RanToCompletion)
            {
                return task;
            }

            return task.ContinueWith(
                new Func<Task<T>, Task<T>>((Task<T> runningTask) =>
                {
                    // Success case.
                    if (!runningTask.IsFaulted || cancellationToken.IsCancellationRequested)
                    {
                        return runningTask;
                    }

                    Exception executedException = runningTask.Exception?.InnerException;
                    TimeSpan delay;
                    bool isTransient = IsSubtypeOf(executedException, transientExceptions);
                    bool shouldStop = retryStrategy(attemptCount, executedException, out delay);
                    if (isTransient == false || shouldStop)
                    {
                        var tcs1 = new TaskCompletionSource<T>();
                        if (executedException != null)
                        {
                            tcs1.TrySetException(executedException);
                        }
                        else
                        {
                            tcs1.TrySetCanceled();
                        }
                        return tcs1.Task;
                    }

                    if (delay.TotalMilliseconds > 0)
                    {
#if NETSTANDARD1_2 || NETSTANDARD1_6 || NETSTANDARD2_0
                        System.Threading.Tasks.Task.Delay((int)delay.TotalMilliseconds).Wait();
#else
                        Thread.Sleep((int)delay.TotalMilliseconds);
#endif
                    }

                    return RetryAsyncInternal(action, ++attemptCount, retryStrategy, cancellationToken, transientExceptions);
                }),
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default
            ).Unwrap();
        }
#endif

#if NET40
        /// <summary>
        /// Provides the async implementation of the retry mechanism for unreliable actions and transient conditions.
        /// </summary>
        /// <param name="action">Unreliable action to execute.</param>
        /// <param name="retryStrategy">Retry strategy.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="transientExceptions">Transient exceptions.</param>
        /// <returns>Task that specified when action executed successfully or with error after all retries.</returns>
        public static Task RetryAsync(
            Func<Task> action,
            RetryStrategy retryStrategy,
            CancellationToken cancellationToken = default(CancellationToken),
            params Type[] transientExceptions)
        {
            Guard.IsNotNull(action, nameof(action));
            Guard.IsNotNull(retryStrategy, nameof(retryStrategy));

            // Try to convert generic task to non-generic.
            Func<Task<int>> nonGenericAction = () =>
            {
                var tcs = new TaskCompletionSource<int>();
                action().ContinueWith(t =>
                {
                    if (t.IsFaulted && t.Exception != null)
                    {
                        tcs.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else if (t.IsCompleted)
                    {
                        tcs.SetResult(0);
                    }
                }, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
                return tcs.Task;
            };
            return RetryAsyncInternal<int>(nonGenericAction, 1, retryStrategy, cancellationToken, transientExceptions);
        }
#endif

#if NET452 || NET461 || NETSTANDARD1_2 || NETSTANDARD1_6 || NETSTANDARD2_0
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

            // Based on TPL police we should check whether action already cancelled.
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
#endif

#if NET452 || NET461 || NETSTANDARD1_2 || NETSTANDARD1_6 || NETSTANDARD2_0
        /// <summary>
        /// Provides the async implementation of the retry mechanism for unreliable actions and transient conditions.
        /// </summary>
        /// <param name="action">Unreliable action to execute.</param>
        /// <param name="retryStrategy">Retry strategy.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="transientExceptions">Transient exceptions.</param>
        /// <returns>Task that specified when action executed successfully or with error after all retries.</returns>
        public static async Task RetryAsync(
            Func<Task> action,
            RetryStrategy retryStrategy,
            CancellationToken cancellationToken = default(CancellationToken),
            params Type[] transientExceptions)
        {
            await RetryAsync<int>(async () =>
            {
                await action();
                return 0;
            }, retryStrategy, cancellationToken, transientExceptions);
        }
#endif

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
#if NETSTANDARD1_2 || NETSTANDARD1_6 || NETSTANDARD2_0
                if (executedExceptionType == exceptionType || executedExceptionType.GetTypeInfo().IsSubclassOf(exceptionType))
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
        /// <param name="firstFastRetry">Make first attempt with no delay. This adds extra attempt.</param>
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
                if (firstFastRetry)
                {
                    attemptCount--;
                }

                if (attemptCount >= numberOfTries)
                {
                    neededDelay = TimeSpan.Zero;
                    return true;
                }

                neededDelay = attemptCount == 0 && firstFastRetry ? TimeSpan.Zero : delay.Value;
                return false;
            };
        }

        /// <summary>
        /// Creates incremental wait time retry strategy. Wait time is incremented by "increment" time.
        /// </summary>
        /// <param name="numberOfTries">Maximum number of tries, default is 3.</param>
        /// <param name="delay">Delay between calls. Default is 0.</param>
        /// <param name="increment">Increment time with every call. Default is 0.</param>
        /// <param name="firstFastRetry">Make first attempt with no delay. This adds extra attempt.</param>
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
                if (firstFastRetry)
                {
                    attemptCount--;
                }

                if (attemptCount >= numberOfTries)
                {
                    neededDelay = TimeSpan.Zero;
                    return true;
                }

                neededDelay = attemptCount == 0 && firstFastRetry ?
                    TimeSpan.Zero :
                    TimeSpan.FromMilliseconds(delay.Value.TotalMilliseconds + (attemptCount * increment.Value.TotalMilliseconds));
                return false;
            };
        }

        /// <summary>
        /// A retry strategy with backoff parameters for calculating the exponential delay between retries.
        /// </summary>
        /// <param name="numberOfTries">Maximum number of tries, default is 3.</param>
        /// <param name="minBackoff">The minimum backoff time, default is 1.</param>
        /// <param name="maxBackoff">The maximum backoff time, default is 30.</param>
        /// <param name="deltaBackoff">The value that will be used to calculate a random delta in the exponential delay between retries.
        /// Disabled by default.</param>
        /// <param name="firstFastRetry">Make first attempt with no delay. This adds extra attempt.</param>
        /// <param name="randomizeDeltaBackoff">Pick random value for deltaBackoff between deltaBackoff * 0.8 and
        /// deltaBackoff * 1.2 . True by default.</param>
        /// <returns>Retry strategy delegate.</returns>
        /// <remarks>
        /// See also
        /// https://github.com/MicrosoftArchive/transient-fault-handling-application-block/blob/master/source/Source/TransientFaultHandling/ExponentialBackoff.cs#L78 .
        /// </remarks>
        public static RetryStrategy CreateExponentialBackoffDelayRetryStrategy(
            int numberOfTries = 3,
            TimeSpan? minBackoff = null,
            TimeSpan? maxBackoff = null,
            TimeSpan? deltaBackoff = null,
            bool firstFastRetry = false,
            bool randomizeDeltaBackoff = true)
        {
            if (minBackoff == null)
            {
                minBackoff = TimeSpan.FromSeconds(1.0);
            }
            if (maxBackoff == null)
            {
                maxBackoff = TimeSpan.FromSeconds(30.0);
            }
            Guard.IsNotNegativeOrZero(numberOfTries, nameof(numberOfTries));
            Guard.IsNotNegative(minBackoff.Value, nameof(minBackoff));
            Guard.IsNotNegative(maxBackoff.Value, nameof(maxBackoff));
            if (deltaBackoff.HasValue)
            {
                Guard.IsNotNegative(deltaBackoff.Value, nameof(deltaBackoff));
            }
            else
            {
                deltaBackoff = TimeSpan.FromSeconds(1.0);
            }
            if (minBackoff > maxBackoff)
            {
                throw new ArgumentOutOfRangeException(nameof(minBackoff),
                    string.Format(Properties.Strings.ArgumentMustBeGreaterThan, nameof(minBackoff), nameof(maxBackoff)));
            }

            return (int attemptCount, Exception lastException, out TimeSpan neededDelay) =>
            {
                // Skip first attempt if it is first fail fast.
                if (firstFastRetry)
                {
                    attemptCount--;
                }

                if (attemptCount >= numberOfTries)
                {
                    neededDelay = TimeSpan.Zero;
                    return true;
                }

                if (attemptCount == 0 && firstFastRetry)
                {
                    neededDelay = TimeSpan.Zero;
                }
                else
                {
                    // delta = (2 ^ attemptCount - 1)
                    // delta = delta * random(deltaBackoff * 0.8, deltaBackoff * 1.2)
                    // interval = min(minBackoff + delta, maxBackoff)
                    double delta = Math.Pow(2.0, attemptCount) - 1.0;
                    if (randomizeDeltaBackoff)
                    {
                        Random random = new Random();
                        delta *= random.Next((int)(deltaBackoff.Value.TotalMilliseconds * 0.8), (int)(deltaBackoff.Value.TotalMilliseconds * 1.2));
                    }
                    else
                    {
                        delta *= deltaBackoff.Value.TotalMilliseconds;
                    }
                    int interval = (int)Math.Min(minBackoff.Value.TotalMilliseconds + delta, maxBackoff.Value.TotalMilliseconds);
                    neededDelay = TimeSpan.FromMilliseconds(interval);
                }
                return false;
            };
        }

        /// <summary>
        /// A retry strategy with backoff parameters for calculating the exponential delay between retries. Normalized
        /// version scales exponential delay depends on numberOfTries.
        /// </summary>
        /// <param name="numberOfTries">Maximum number of tries, default is 3.</param>
        /// <param name="minBackoff">The minimum backoff time, default is 1.</param>
        /// <param name="maxBackoff">The maximum backoff time, default is 30.</param>
        /// <param name="firstFastRetry">Make first attempt with no delay. This adds extra attempt.</param>
        /// <returns>Retry strategy delegate.</returns>
        public static RetryStrategy CreateExponentialBackoffNormalizedDelayRetryStrategy(
            int numberOfTries = 3,
            TimeSpan? minBackoff = null,
            TimeSpan? maxBackoff = null,
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
            Guard.IsNotNegativeOrZero(numberOfTries, nameof(numberOfTries));
            Guard.IsNotNegative(minBackoff.Value, nameof(minBackoff));
            Guard.IsNotNegative(maxBackoff.Value, nameof(maxBackoff));

            var deltaBackoff = (maxBackoff.Value.TotalMilliseconds - minBackoff.Value.TotalMilliseconds) / Math.Pow(2, numberOfTries - 1);
            return CreateExponentialBackoffDelayRetryStrategy(numberOfTries, minBackoff, maxBackoff,
                deltaBackoff: TimeSpan.FromMilliseconds(deltaBackoff),
                firstFastRetry: firstFastRetry,
                randomizeDeltaBackoff: false);
        }

        /// <summary>
        /// Creates wrapper delegate around "RetryCallback". Can be used for logging or debug purpose.
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
                    // Ignored.
                }
                return false;
            };
        }

        #endregion
    }
}
