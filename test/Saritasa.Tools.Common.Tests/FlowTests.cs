// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Extensions;
    using Utils;

    /// <summary>
    /// Flow, enumerable tests.
    /// </summary>
    public class FlowTests
    {
        private event EventHandler<EventArgs> EventArgsTestEvent;

        [Fact]
        public void Raise_should_call_test_handler()
        {
            int a = 10;
            EventArgs eventArgs = new EventArgs();
            EventHandler<EventArgs> testDelegate = (sender, args) =>
            {
                a = 20;
            };

            EventArgsTestEvent = null;
            EventArgsTestEvent += testDelegate;
            FlowUtils.Raise(this, eventArgs, ref EventArgsTestEvent);
            Assert.Equal(20, a);
        }

        [Fact]
        public void Raise_all_should_call_test_handlers()
        {
            int a = 0;
            EventArgs eventArgs = new EventArgs();
            EventHandler<EventArgs> testDelegate = (sender, args) =>
            {
                a += 1;
            };

            EventArgsTestEvent = null;
            EventArgsTestEvent += testDelegate;
            EventArgsTestEvent += testDelegate;
            EventArgsTestEvent += testDelegate;
            FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent);
            Assert.Equal(3, a);
        }

        [Fact]
        public void Raise_all_should_return_exception_of_all_handlers()
        {
            int a = 10;
            EventArgs eventArgs = new EventArgs();
            EventHandler<EventArgs> testDelegate = (sender, args) =>
            {
                a = 20;
            };
            EventHandler<EventArgs> testDelegate2 = (sender, args) =>
            {
                a = 30;
                throw new Exception("test");
            };

            EventArgsTestEvent += testDelegate;
            EventArgsTestEvent += testDelegate2;
            Assert.Throws<AggregateException>(() => { FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent); });
            Assert.Equal(30, a);
        }

        [Fact]
        public void Raise_should_not_throw_if_event_delegate_is_null()
        {
            EventArgsTestEvent = null;
            EventArgs eventArgs = new EventArgs();
            FlowUtils.Raise(this, eventArgs, ref EventArgsTestEvent);
            FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent);
        }

        [Fact]
        public void Paged_enumerable_should_return_correct_enumerables_for_pages()
        {
            int capacity = 250;
            IList<int> list = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }
            var pagedList = new PagedEnumerable<int>(list, 10, 10);
            Assert.Equal(25, pagedList.TotalPages);

            var pagedList2 = new PagedEnumerable<int>(list, 10, 10);
            Assert.Equal(10, pagedList2.Count());

            var pagedList3 = list.GetPaged(13, 25);
            Assert.Equal(10, pagedList3.TotalPages);
            Assert.Equal(13, pagedList3.CurrentPage);

            var pagedList4 = list.GetPaged(20, 13);
            Assert.Equal(20, pagedList4.TotalPages);
            Assert.Equal(3, pagedList4.Count());
        }

        /// <summary>
        /// Custom exception for tests only.
        /// </summary>
        private class CustomException : Exception
        {
        }

        private void CustomMethodNoReturn()
        {
        }

        private int CustomMethodReturn()
        {
            return 123;
        }

        private int CustomMethodReturnWithCustomException()
        {
            throw new CustomException();
        }

        [Fact]
        public void Repeat_with_fixed_retry_strategy_should_work()
        {
            // fixed retry
            FlowUtils.Retry(CustomMethodReturn, FlowUtils.CreateFixedDelayRetryStrategy());
            FlowUtils.Retry(CustomMethodReturn, FlowUtils.CreateFixedDelayRetryStrategy(int.MaxValue, TimeSpan.MaxValue));
        }

        [Fact]
        public void Repeat_with_fixed_retry_strategy_should_throw_exceptions()
        {
            // fixed throw exception
            FlowUtils.Retry(CustomMethodReturnWithCustomException, FlowUtils.CreateFixedDelayRetryStrategy());
            Assert.Throws<CustomException>(() => FlowUtils.Retry(CustomMethodReturnWithCustomException, FlowUtils.CreateFixedDelayRetryStrategy(), typeof(InvalidOperationException)));
            FlowUtils.Retry(CustomMethodReturnWithCustomException, FlowUtils.CreateFixedDelayRetryStrategy(), typeof(CustomException));
        }

        [Fact]
        public void Repeat_with_fixed_retry_strategy_should_delay_correctly()
        {
            // fixed delay
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            FlowUtils.Retry(CustomMethodReturnWithCustomException, FlowUtils.CreateFixedDelayRetryStrategy(3, TimeSpan.FromMilliseconds(50)), typeof(CustomException));
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 100);
        }

        [Fact]
        public void Repeat_with_fixed_retry_strategy_and_first_fast_should_work()
        {
            // fixed delay
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            FlowUtils.Retry(
                CustomMethodReturnWithCustomException,
                FlowUtils.CreateFixedDelayRetryStrategy(4, TimeSpan.FromMilliseconds(50), true),
                typeof(CustomException)
            );
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 100);
        }

        [Fact]
        public void Repeat_with_log_handler_should_log()
        {
            int totalAttempts = 0;
            var callback = new FlowUtils.RetryCallback((a, e) =>
            {
                totalAttempts = a;
            });
            FlowUtils.Retry(
                CustomMethodReturnWithCustomException,
                FlowUtils.CreateCallbackRetryStrategy(callback) + FlowUtils.CreateFixedDelayRetryStrategy(2)
            );
            Assert.Equal(2, totalAttempts);
        }

        private Task<int> CustomMethodReturnWithCustomExceptionAsync()
        {
            return Task.Factory.StartNew<int>(() =>
            {
                throw new CustomException();
            });
        }

        [Fact]
        public void Repeat_async_with_fixed_retry_strategy_should_delay_correctly()
        {
            // fixed async delay
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var task = FlowUtils.RetryAsync(
                CustomMethodReturnWithCustomExceptionAsync,
                FlowUtils.CreateFixedDelayRetryStrategy(3, TimeSpan.FromMilliseconds(50)),
                CancellationToken.None,
                typeof(CustomException));
            try
            {
                task.Wait();
            }
            catch (AggregateException)
            {
            }
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 100);
        }

        [Fact]
        public void Repeat_async_with_fixed_retry_strategy_and_first_fast_should_work()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                FlowUtils.Retry(
                    CustomMethodReturnWithCustomExceptionAsync,
                    FlowUtils.CreateFixedDelayRetryStrategy(2, TimeSpan.FromMilliseconds(50), true),
                    typeof(CustomException)).Wait();
            }
            catch (AggregateException)
            {
            }
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds <= 50);
        }

        [Fact]
        public void Repeat_async_with_log_handler_should_log()
        {
            int totalAttempts = 0;
            var callback = new FlowUtils.RetryCallback((a, e) =>
            {
                totalAttempts = a;
            });
            try
            {
                FlowUtils.RetryAsync(
                    CustomMethodReturnWithCustomExceptionAsync,
                    FlowUtils.CreateCallbackRetryStrategy(callback) + FlowUtils.CreateFixedDelayRetryStrategy(2)).Wait();
            }
            catch (AggregateException)
            {
            }
            Assert.Equal(2, totalAttempts);
        }

        [Fact]
        public void Repeat_async_with_first_fail_handler_should_work_fine()
        {
            int totalAttempts = 0;
            var result = FlowUtils.RetryAsync(
                () =>
                {
                    return Task.Factory.StartNew(() =>
                    {
                        totalAttempts++;
                        if (totalAttempts < 5)
                        {
                            throw new CustomException();
                        }
                        return 10;
                    });
                },
                FlowUtils.CreateFixedDelayRetryStrategy(8)
            ).Result;
            Assert.Equal(10, result);
            Assert.True(totalAttempts > 4);
        }

        [Fact]
        public void Memoize_with_default_dict_should_call_handler_once()
        {
            int value = 0;
            var memoized1 = FlowUtils.Memoize(() => ++value);

            Assert.Equal(1, memoized1());
            Assert.Equal(1, memoized1());
        }

        [Fact]
        public void Memoize_with_skipmemoize_exception_should_not_cache()
        {
            int value = 0;
            var memoized1 = FlowUtils.Memoize(
                new Func<int>(() =>
                {
                    throw new FlowUtils.SkipMemoizeException<int>(++value);
                })
            );

            Assert.Equal(1, memoized1());
            Assert.Equal(2, memoized1());
        }

        [Fact]
        public void Cache_with_max_age_should_cache_within_period()
        {
            int value = 0;
            var memoized1 = FlowUtils.Memoize(
                () => ++value,
                FlowUtils.CreateMaxAgeCacheStrategy<int>(TimeSpan.FromSeconds(1))
            );

            Assert.Equal(1, memoized1());
            Assert.Equal(1, memoized1());
#if PORTABLE || NETSTANDARD1_6
            Task.Delay(1300).Wait();
#else
            Thread.Sleep(1300);
#endif
            Assert.Equal(2, memoized1());
        }

        [Fact]
        public void Cache_with_max_count_should_save_within_count()
        {
            int value = 0;
            var memoized1 = FlowUtils.Memoize(
                (a) => ++value,
                FlowUtils.CreateMaxCountCacheStrategy<int, int>(maxCount: 3, removeCount: 2)
            );

            Assert.Equal(1, memoized1(1));
            Assert.Equal(2, memoized1(2));
            Assert.Equal(3, memoized1(3));
            Assert.Equal(1, memoized1(1)); // cached here
            Assert.Equal(4, memoized1(4)); // cache reset here
            Assert.Equal(5, memoized1(1)); // not cached now
            Assert.Equal(3, memoized1(3)); // but this one is cached
        }
    }
}
