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
            // Arrange
            int a = 10;
            EventArgs eventArgs = new EventArgs();
            EventHandler<EventArgs> testDelegate = (sender, args) =>
            {
                a = 20;
            };
            EventArgsTestEvent = null;
            EventArgsTestEvent += testDelegate;

            // Act
            FlowUtils.Raise(this, eventArgs, ref EventArgsTestEvent);

            // Assert
            Assert.Equal(20, a);
        }

        [Fact]
        public void Raise_all_should_call_test_handlers()
        {
            // Arrange
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

            // Act
            FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent);

            // Assert
            Assert.Equal(3, a);
        }

        [Fact]
        public void Raise_all_should_return_exception_of_all_handlers()
        {
            // Arrange
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

            // Act
            Assert.Throws<AggregateException>(() => { FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent); });

            // Assert
            Assert.Equal(30, a);
        }

        [Fact]
        public void Raise_should_not_throw_if_event_delegate_is_null()
        {
            // Arrange
            EventArgsTestEvent = null;
            EventArgs eventArgs = new EventArgs();

            // Act
            FlowUtils.Raise(this, eventArgs, ref EventArgsTestEvent);
            FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent);

            // Assert
        }

        [Fact]
        public void Paged_enumerable_should_return_correct_enumerables_for_pages()
        {
            // Arrange
            int capacity = 250;
            IList<int> list = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }

            // Act
            var pagedList = new PagedEnumerable<int>(list, 10, 10);
            var pagedList2 = new PagedEnumerable<int>(list, 10, 10);
            var pagedList3 = list.GetPaged(13, 25);
            var pagedList4 = list.GetPaged(20, 13);

            // Assert
            Assert.Equal(25, pagedList.TotalPages);
            Assert.Equal(10, pagedList2.Count());
            Assert.Equal(10, pagedList3.TotalPages);
            Assert.Equal(13, pagedList3.CurrentPage);
            Assert.Equal(20, pagedList4.TotalPages);
            Assert.Equal(3, pagedList4.Count());
        }

        /// <summary>
        /// Custom exception for tests only.
        /// </summary>
        private class CustomException : Exception
        {
        }

        [Fact]
        public void Repeat_with_fixed_retry_strategy_should_work()
        {
            // Arrange
            Func<int> customMethodReturn = () => 123;

            // Act & assert
            FlowUtils.Retry(customMethodReturn, FlowUtils.CreateFixedDelayRetryStrategy());
            FlowUtils.Retry(customMethodReturn, FlowUtils.CreateFixedDelayRetryStrategy(int.MaxValue, TimeSpan.MaxValue));
        }

        [Fact]
        public void Repeat_with_fixed_retry_strategy_should_throw_exceptions()
        {
            // Arrange
            Action customMethodReturnWithCustomException = () =>
            {
                throw new CustomException();
            };

            // Act & assert
            Assert.Throws<CustomException>(
                () => FlowUtils.Retry(customMethodReturnWithCustomException,
                    FlowUtils.CreateFixedDelayRetryStrategy()));
            Assert.Throws<CustomException>(
                () => FlowUtils.Retry(customMethodReturnWithCustomException,
                    FlowUtils.CreateFixedDelayRetryStrategy(), typeof(InvalidOperationException)));
            Assert.Throws<CustomException>(
                () => FlowUtils.Retry(customMethodReturnWithCustomException,
                    FlowUtils.CreateFixedDelayRetryStrategy(), typeof(CustomException)));
        }

        [Fact]
        public void Repeat_with_fixed_retry_strategy_should_delay_correctly()
        {
            // Arrange
            Action customMethodReturnWithCustomException = () =>
            {
                throw new CustomException();
            };
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            try
            {
                FlowUtils.Retry(customMethodReturnWithCustomException, FlowUtils.CreateFixedDelayRetryStrategy(3, TimeSpan.FromMilliseconds(50)), typeof(CustomException));
            }
            catch (CustomException)
            {
                // suppress our specific exception
            }
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds >= 100);
        }

        [Fact]
        public void Repeat_with_fixed_retry_strategy_and_first_fast_should_work()
        {
            // Arrange
            Action customMethodReturnWithCustomException = () =>
            {
                throw new CustomException();
            };
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            try
            {
                FlowUtils.Retry(
                    customMethodReturnWithCustomException,
                    FlowUtils.CreateFixedDelayRetryStrategy(4, TimeSpan.FromMilliseconds(50), true),
                    typeof(CustomException)
                );
            }
            catch (CustomException)
            {
                // suppress our specific exception
            }
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds >= 100);
        }

        [Fact]
        public void Repeat_with_log_handler_should_log()
        {
            // Arrange
            Action customMethodReturnWithCustomException = () =>
            {
                throw new CustomException();
            };
            int totalAttempts = 0;
            var callback = new FlowUtils.RetryCallback((a, e) =>
            {
                totalAttempts = a;
            });

            // Act
            try
            {
                FlowUtils.Retry(
                    customMethodReturnWithCustomException,
                    FlowUtils.CreateCallbackRetryStrategy(callback) + FlowUtils.CreateFixedDelayRetryStrategy(2)
                );
            }
            catch (CustomException)
            {
                // suppress our specific exception
            }

            // Assert
            Assert.Equal(2, totalAttempts);
        }

        [Fact]
        public void Repeat_async_with_fixed_retry_strategy_should_delay_correctly()
        {
            // Arrange
            Func<Task<int>> customMethodReturnWithCustomExceptionAsync = () =>
                Task.Factory.StartNew<int>(() => { throw new CustomException(); });
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            var task = FlowUtils.RetryAsync(
                customMethodReturnWithCustomExceptionAsync,
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

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds >= 100);
        }

        [Fact]
        public void Repeat_async_with_fixed_retry_strategy_and_first_fast_should_work()
        {
            // Arrange
            Func<Task<int>> customMethodReturnWithCustomExceptionAsync = () =>
                Task.Factory.StartNew<int>(() => { throw new CustomException(); });
            var stopwatch = new Stopwatch();

            // Act
            stopwatch.Start();
            try
            {
                FlowUtils.Retry(
                    customMethodReturnWithCustomExceptionAsync,
                    FlowUtils.CreateFixedDelayRetryStrategy(2, TimeSpan.FromMilliseconds(50), true),
                    typeof(CustomException)).Wait();
            }
            catch (AggregateException)
            {
            }
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds <= 50);
        }

        [Fact]
        public void Repeat_async_with_log_handler_should_log()
        {
            // Arrange
            Func<Task<int>> customMethodReturnWithCustomExceptionAsync = () =>
                Task.Factory.StartNew<int>(() => { throw new CustomException(); });
            int totalAttempts = 0;
            var callback = new FlowUtils.RetryCallback((a, e) =>
            {
                totalAttempts = a;
            });

            // Act
            try
            {
                FlowUtils.RetryAsync(
                    customMethodReturnWithCustomExceptionAsync,
                    FlowUtils.CreateCallbackRetryStrategy(callback) + FlowUtils.CreateFixedDelayRetryStrategy(2)).Wait();
            }
            catch (AggregateException)
            {
            }

            // Assert
            Assert.Equal(2, totalAttempts);
        }

        [Fact]
        public void Repeat_async_with_first_fail_handler_should_work_fine()
        {
            // Arrange
            int totalAttempts = 0;

            // Act
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

            // Assert
            Assert.Equal(10, result);
            Assert.True(totalAttempts > 4);
        }

        [Fact]
        public void Memoize_with_default_dict_should_call_handler_once()
        {
            // Arrange
            int value = 0;
            var memoized1 = FlowUtils.Memoize(() => ++value);

            // Act & Assert
            Assert.Equal(1, memoized1());
            Assert.Equal(1, memoized1());
        }

        [Fact]
        public void Memoize_with_skipmemoize_exception_should_not_cache()
        {
            // Arrange
            int value = 0;
            var memoized1 = FlowUtils.Memoize(
                new Func<int>(() =>
                {
                    throw new FlowUtils.SkipMemoizeException<int>(++value);
                })
            );

            // Act & Assert
            Assert.Equal(1, memoized1());
            Assert.Equal(2, memoized1());
        }

        [Fact]
        public void Cache_with_max_age_should_cache_within_period()
        {
            // Arrange
            int value = 0;
            var memoized1 = FlowUtils.Memoize(
                () => ++value,
                FlowUtils.CreateMaxAgeCacheStrategy<int>(TimeSpan.FromSeconds(1))
            );

            // Act & assert
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
            // Arrange
            int value = 0;
            var memoized1 = FlowUtils.Memoize(
                (a) => ++value,
                FlowUtils.CreateMaxCountCacheStrategy<int, int>(maxCount: 3, removeCount: 2)
            );

            // Act & assert
            Assert.Equal(1, memoized1(1));
            Assert.Equal(2, memoized1(2));
            Assert.Equal(3, memoized1(3));
            Assert.Equal(1, memoized1(1)); // cached here
            Assert.Equal(4, memoized1(4)); // cache reset here
            Assert.Equal(5, memoized1(1)); // not cached now
            Assert.Equal(3, memoized1(3)); // but this one is cached
        }

        [Fact]
        public void Cache_with_composite_key_should_cache()
        {
            // Arrange
            int totalCalls = 0;
            Func<int, string, int> func = (a, b) => ++totalCalls;
            var memoizedFunc = FlowUtils.Memoize(
                func, FlowUtils.CreateMaxCountCacheStrategy<int, string, int>(100));

            // Act
            var val1 = memoizedFunc(10, "string");
            var val2 = memoizedFunc(10, "string");
            var val3 = memoizedFunc(10, "string2");
            var val4 = memoizedFunc(11, "string");
            var val5 = memoizedFunc(10, "string");
            var val6 = memoizedFunc(11, "string");

            // Assert
            Assert.Equal(1, val1);
            Assert.Equal(1, val2);
            Assert.Equal(2, val3);
            Assert.Equal(3, val4);
            Assert.Equal(1, val5);
            Assert.Equal(3, val6);
        }
    }
}
