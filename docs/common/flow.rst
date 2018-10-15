Flow
====

Methods that affect application execution flow.

.. function:: T Retry<T>(Func<T> action, RetryStrategy retryStrategy, params Type[] transientExceptions)
.. function:: Task<T> RetryAsync<T>(Func<Task<T>> action, RetryStrategy retryStrategy, CancellationToken cancellationToken, params Type[] transientExceptions)

    Provides the sync/async implementation of the retry mechanism for unreliable actions and transient conditions. Please note that having ``firstFastRetry`` would add extra attempt to total number of tries. There are following retry strategies:

    .. function:: CreateFixedDelayRetryStrategy(int numberOfTries, TimeSpan? delay, bool firstFastRetry)

        There will be fixed time delay between every failed action call. One of the simplest strategies. Here is a sample run with delays for configuration ``numberOfTries=10``, ``delay=2 sec``, no first fast retry:

            ::

                #   delay     total time
                01: 00:00.00  00:00.00
                02: 00:02.01  00:02.02
                03: 00:02.01  00:04.03
                04: 00:02.02  00:06.06
                05: 00:02.03  00:08.09
                06: 00:02.02  00:10.12
                07: 00:02.02  00:12.14
                08: 00:02.03  00:14.17
                09: 00:02.03  00:16.21
                10: 00:02.03  00:18.24

            .. image:: flow-retry-fixed.png

    .. function:: CreateIncrementDelayRetryStrategy(int numberOfTries, TimeSpan? delay, TimeSpan? increment, bool firstFastRetry)

        There will be incremented delay between every failed action call. Here is a sample run with delays for configuration ``numberOfTries=10``, ``delay=2 sec``, ``increment=1 sec``, no first fast retry:

            ::

                #   delay     total time
                01: 00:00.00  00:00.00
                02: 00:03.01  00:03.02
                03: 00:04.02  00:07.04
                04: 00:05.03  00:12.08
                05: 00:06.03  00:18.11
                06: 00:07.02  00:25.14
                07: 00:08.03  00:33.17
                08: 00:09.03  00:42.20
                09: 00:10.01  00:52.22
                10: 00:11.02  01:03.25

            .. image:: flow-retry-incremental.png

    .. function:: CreateExponentialBackoffDelayRetryStrategy(int numberOfTries, TimeSpan? minBackoff, TimeSpan? maxBackoff, TimeSpan? deltaBackoff, bool firstFastRetry, bool randomizeDeltaBackoff)

        A retry strategy with backoff parameters for calculating the exponential delay between retries. Delta backoff (jitter) requires to randomize next delay. The implementation is equal to Microsoft Enterprise Library exponential backoff transient fault handling. Here is a sample run with delays for configuration ``numberOfTries=10``, ``minBackoff=2 sec``, ``maxBackoff=35 sec``, no delta backoff, no first fast retry:

            ::

                #   delay     total time
                01: 00:00.00  00:00.00
                02: 00:03.01  00:03.02
                03: 00:05.03  00:08.05
                04: 00:09.03  00:17.09
                05: 00:17.03  00:34.12
                06: 00:33.02  01:07.14
                07: 00:35.02  01:42.17
                08: 00:35.03  02:17.21
                09: 00:35.03  02:52.24
                10: 00:35.02  03:27.26

            .. image:: flow-retry-exp.png

        You can add randomization and change the delay behavior with ``deltaBackoff`` parameter. Make it more aggressive or optimistic. For example the same graph with delta backoff 0.5 sec:

            .. image:: flow-retry-exp2.png

    .. function:: CreateExponentialBackoffNormalizedDelayRetryStrategy(int numberOfTries, TimeSpan? minBackoff, TimeSpan? maxBackoff, bool firstFastRetry)

        A retry strategy with backoff parameters for calculating the exponential delay between retries. Normalized version scales exponential delay depends on ``numberOfTries``. Here is a sample run with delays for configuration ``numberOfTries=10``, ``minBackoff=2`` sec, ``maxBackoff=35`` sec, no first fast retry:

            ::

                #   delay     total time
                01: 00:00.00  00:00.00
                02: 00:02.09  00:02.10
                03: 00:02.20  00:04.30
                04: 00:02.46  00:06.76
                05: 00:02.98  00:09.75
                06: 00:04.03  00:13.78
                07: 00:06.10  00:19.88
                08: 00:10.21  00:30.10
                09: 00:18.47  00:48.58
                10: 00:35.03  01:23.61

            .. image:: flow-retry-expnormalized.png

        It is the same as exponential backoff strategy but ``deltaBackoff`` is calculates as

            ::

                deltaBackoff = (maxBackoff - minBackoff) / 2^(numberOfTries-1)

    .. function:: CreateCallbackRetryStrategy(RetryCallback callback)

        Creates wrapper delegate around "RetryCallback". Can be used for loggin or debug purpose. Please note that this delegate should be passed first when combine with RetryStrategyDelegate.

    Here are several examples of usage:

    .. code-block:: c#

            // Async repeat with fixed delay strategy.
            FlowUtils.RetryAsync(() =>
                {
                    return SendEmail("email");
                },
                FlowUtils.CreateFixedDelayRetryStrategy(12),
                CancellationToken.None,
                typeof(SmtpException)
            );

            // Use increment delay retry strategy with logging.
            FlowUtils.Retry(() =>
                {
                    // Action.
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
                        return value; // Some processing here.
                    }),
                    FlowUtils.CreateMaxCountCacheStrategy<int, int>(maxCount: 3, removeCount: 2)
                );

.. function:: void RaiseAll<TEventArgs>(object sender, TEventArgs e, ref EventHandler<TEventArgs> eventDelegate)

    Helps to raise event for all handlers. If any exception would occure the `AggregateException` will be thrown.
