Flow
====

Method to affect application flow.

.. function:: T Retry<T>(Func<T> action, RetryStrategy retryStrategy, params Type[] transientExceptions)
.. function:: Task<T> RetryAsync<T>(Func<Task<T>> action, RetryStrategy retryStrategy, CancellationToken cancellationToken, params Type[] transientExceptions)

    Provides the sync/async implementation of the retry mechanism for unreliable actions and transient conditions. There are following retry strategies:

    .. function:: CreateFixedDelayRetryStrategy(int numberOfTries, TimeSpan? delay, bool firstFastRetry)

        There will be fixed time delay between every failed action call.

    .. function:: CreateIncrementDelayRetryStrategy(int numberOfTries, TimeSpan? delay, TimeSpan? increment, bool firstFastRetry)

        There will be incremented delay between every failed action call.

    .. function::  CreateExponentialBackoffDelayRetryStrategy(int numberOfTries, TimeSpan? minBackoff, TimeSpan? maxBackoff, TimeSpan? deltaBackoff, bool firstFastRetry)

        A retry strategy with backoff parameters for calculating the exponential delay between retries.

    .. function:: CreateCallbackRetryStrategy(RetryCallback callback)

        Creates wrapper delegate around "RetryCallback". Can be used for loggin or debug purpose. Please note that this delegate should be passed first when combine with RetryStrategyDelegate.

    Here are several examples of usage:

    .. code-block:: c#

            // async repeat with fixed delay strategy
            FlowUtils.RetryAsync(() =>
                {
                    return SendEmail("email");
                },
                FlowUtils.CreateFixedDelayRetryStrategy(12),
                CancellationToken.None,
                typeof(SmtpException)
            );

            // use increment delay retry strategy with logging
            FlowUtils.Retry(() =>
                {
                    // action
                },
                FlowUtils.CreateCallbackRetryStrategy((attempt, ex) =>
                {
                    Console.WriteLine("Log: {0}", ex);
                }) +
                FlowUtils.CreateIncrementDelayRetryStrategy(12, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), true)
            );

.. function:: Func<TKey, TResult> Memoize<TKey, TResult>(Func<TKey, TResult> func, CacheStrategy<TKey, TResult> strategies, IDictionary<TKey, TResult> cache)

    Returns a memoized version of a referentially transparent function. The memoized version of the function keeps a cache of the mapping from arguments to results and, when calls with the same arguments are repeated often, has higher performance at the expense of higher memory use.

    The following cache strategies can be used:

    .. function:: CacheStrategy<TKey, TResult> CreateMaxCountCacheStrategy<TKey, TResult>(int maxCount, int removeCount, bool purge, IList<TKey> keysStorage)

    .. function:: CacheStrategy<TKey, TResult> CreateMaxAgeCacheStrategy<TKey, TResult>(TimeSpan maxAge, IDictionary<TKey, DateTime> timestampsStorage)

        Example of usage:

            .. code-block:: c#

                var memoized1 = FlowUtils.Memoize(
                    new Func<int, int>((int a) =>
                    {
                        return value; // some processing here
                    }),
                    FlowUtils.CreateMaxCountCacheStrategy<int, int>(maxCount: 3, removeCount: 2)
                );

.. function:: void Raise<TEventArgs>(object sender, TEventArgs e, ref EventHandler<TEventArgs> eventDelegate)

    Helps to raise event handlers.

    If you develop your own class with events it is not handy to raise it. You should check whther it is null. Even in that case your code is not thread safe. This method makes these two checks and calls event. Example:

        .. code-block:: c#

            // without Saritasa extensions, not thread safe
            if (TestEvent != null)
                TestEvent(sender, eventArgs);

            // with Saritasa extensions
            FlowsUtils.Raise(eventArgs, sender, ref TestEvent);

.. function:: void RaiseAll<TEventArgs>(object sender, TEventArgs e, ref EventHandler<TEventArgs> eventDelegate)

    Helps to raise event for all handlers. If any exception would occure the `AggregateException` will be thrown.
