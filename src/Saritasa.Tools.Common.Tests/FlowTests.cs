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
    using NUnit.Framework;
    using Extensions;
    using Utils;

    /// <summary>
    /// Flow, enumerable tests.
    /// </summary>
    [TestFixture]
    public class FlowTests
    {
        private event EventHandler<EventArgs> EventArgsTestEvent;

        [Test]
        public void Raise_should_call_test_handler()
        {
            int a = 10;
            EventArgs eventArgs = new EventArgs();
            EventHandler<EventArgs> testDelegate = (sender, args) =>
            {
                a = 20;
            };

            try
            {
                EventArgsTestEvent = null;
                EventArgsTestEvent += testDelegate;
                FlowUtils.Raise(this, eventArgs, ref EventArgsTestEvent);
                Assert.That(a, Is.EqualTo(20));
            }
            finally
            {
                EventArgsTestEvent = null;
            }
        }

        [Test]
        public void Raise_all_should_call_test_handlers()
        {
            int a = 0;
            EventArgs eventArgs = new EventArgs();
            EventHandler<EventArgs> testDelegate = (sender, args) =>
            {
                a += 1;
            };

            try
            {
                EventArgsTestEvent = null;
                EventArgsTestEvent += testDelegate;
                EventArgsTestEvent += testDelegate;
                EventArgsTestEvent += testDelegate;
                FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent);
                Assert.That(a, Is.EqualTo(3));
            }
            finally
            {
                EventArgsTestEvent = null;
            }
        }

        [Test]
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

            try
            {
                EventArgsTestEvent += testDelegate;
                EventArgsTestEvent += testDelegate2;
#if !NET35
                Assert.Throws<AggregateException>(() => { FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent); });
#else
                Assert.Throws<Exception>(() => { FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent); });
#endif
                Assert.That(a, Is.EqualTo(30));
            }
            finally
            {
                EventArgsTestEvent = null;
            }
        }

        [Test]
        public void Raise_should_not_throw_if_event_delegate_is_null()
        {
            EventArgsTestEvent = null;
            EventArgs eventArgs = new EventArgs();
            FlowUtils.Raise(this, eventArgs, ref EventArgsTestEvent);
            FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent);
        }

        [Test]
        public void Paged_enumerable_should_return_correct_enumerables_for_pages()
        {
            int capacity = 250;
            IList<int> list = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }
            var pagedList = new PagedEnumerable<int>(list, 10, 10);
            Assert.That(pagedList.TotalPages, Is.EqualTo(25));

            var pagedList2 = new PagedEnumerable<int>(list, 10, 10);
            Assert.That(pagedList2.Count(), Is.EqualTo(10));

            var pagedList3 = list.GetPaged(13, 25);
            Assert.That(pagedList3.TotalPages, Is.EqualTo(10));
            Assert.That(pagedList3.CurrentPage, Is.EqualTo(13));

            var pagedList4 = list.GetPaged(20, 13);
            Assert.That(pagedList4.TotalPages, Is.EqualTo(20));
            Assert.That(pagedList4.Count(), Is.EqualTo(3));
        }

        /// <summary>
        /// Custom exception for tests only.
        /// </summary>
        [Serializable]
        private class CustomException : Exception
        {
            public CustomException()
            {
            }
        }

        private void CustomMethodNoReturn()
        {
        }

        private Int32 CustomMethodReturn()
        {
            return 123;
        }

        private Int32 CustomMethodReturnWithCustomException()
        {
            throw new CustomException();
        }

        [Test]
        public void Repeat_with_fixed_retry_strategy_should_work()
        {
            // fixed retry
            FlowUtils.Retry(CustomMethodReturn, FlowUtils.CreateFixedDelayRetryStrategy());
            FlowUtils.Retry(CustomMethodReturn, FlowUtils.CreateFixedDelayRetryStrategy(Int32.MaxValue, TimeSpan.MaxValue));
        }

        [Test]
        public void Repeat_with_fixed_retry_strategy_should_throw_exceptions()
        {
            // fixed throw exception
            FlowUtils.Retry(CustomMethodReturnWithCustomException, FlowUtils.CreateFixedDelayRetryStrategy());
            Assert.Throws<CustomException>(() => FlowUtils.Retry(CustomMethodReturnWithCustomException, FlowUtils.CreateFixedDelayRetryStrategy(), typeof(InvalidOperationException)));
            FlowUtils.Retry(CustomMethodReturnWithCustomException, FlowUtils.CreateFixedDelayRetryStrategy(), typeof(CustomException));
        }

        [Test]
        public void Repeat_with_fixed_retry_strategy_should_delay_correctly()
        {
            // fixed delay
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            FlowUtils.Retry(CustomMethodReturnWithCustomException, FlowUtils.CreateFixedDelayRetryStrategy(3, TimeSpan.FromMilliseconds(50)), typeof(CustomException));
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 100);
        }

        [Test]
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

        [Test]
        public void Repeat_with_log_handler_should_log()
        {
            Int32 totalAttempts = 0;
            var callback = new FlowUtils.RetryCallback((a, e) =>
            {
                totalAttempts = a;
            });
            FlowUtils.Retry(
                CustomMethodReturnWithCustomException,
                FlowUtils.CreateCallbackRetryStrategy(callback) + FlowUtils.CreateFixedDelayRetryStrategy(2)
            );
            Assert.That(totalAttempts, Is.EqualTo(2));
        }

#if !NET35

        private Task<int> CustomMethodReturnWithCustomExceptionAsync()
        {
            return Task.Factory.StartNew<int>(() =>
            {
                throw new CustomException();
            });
        }

        [Test]
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

        [Test]
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

        [Test]
        public void Repeat_async_with_log_handler_should_log()
        {
            Int32 totalAttempts = 0;
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
            Assert.That(totalAttempts, Is.EqualTo(2));
        }

        [Test]
        public void Repeat_async_with_first_fail_handler_should_work_fine()
        {
            Int32 totalAttempts = 0;
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
            Assert.That(result, Is.EqualTo(10));
            Assert.That(totalAttempts, Is.GreaterThan(4));
        }
#endif

        [Test]
        public void Memoize_with_default_dict_should_call_handler_once()
        {
            int value = 0;
            var memoized1 = FlowUtils.Memoize(
                new Func<int>(() =>
                {
                    return ++value;
                }));

            Assert.That(memoized1(), Is.EqualTo(1));
            Assert.That(memoized1(), Is.EqualTo(1));
        }

        [Test]
        public void Memoize_with_skipmemoize_exception_should_not_cache()
        {
            int value = 0;
            var memoized1 = FlowUtils.Memoize(
                new Func<int>(() =>
                {
                    throw new FlowUtils.SkipMemoizeException<int>(++value);
                })
            );

            Assert.That(memoized1(), Is.EqualTo(1));
            Assert.That(memoized1(), Is.EqualTo(2));
        }

        [Test]
        public void Cache_with_max_age_should_cache_within_period()
        {
            int value = 0;
            var memoized1 = FlowUtils.Memoize(
                new Func<Int32>(() =>
                {
                    return ++value;
                }),
                FlowUtils.CreateMaxAgeCacheStrategy<Int32>(TimeSpan.FromSeconds(1))
            );

            Assert.That(memoized1(), Is.EqualTo(1));
            Assert.That(memoized1(), Is.EqualTo(1));
#if PORTABLE || NETSTANDARD1_6
            Task.Delay(1300).Wait();
#else
            Thread.Sleep(1300);
#endif
            Assert.That(memoized1(), Is.EqualTo(2));
        }

        [Test]
        public void Cache_with_max_count_should_save_within_count()
        {
            int value = 0;
            var memoized1 = FlowUtils.Memoize(
                new Func<int, int>((int a) =>
                {
                    return ++value;
                }),
                FlowUtils.CreateMaxCountCacheStrategy<int, int>(maxCount: 3, removeCount: 2)
            );

            Assert.That(memoized1(1), Is.EqualTo(1));
            Assert.That(memoized1(2), Is.EqualTo(2));
            Assert.That(memoized1(3), Is.EqualTo(3));
            Assert.That(memoized1(1), Is.EqualTo(1)); // cached here
            Assert.That(memoized1(4), Is.EqualTo(4)); // cache reset here
            Assert.That(memoized1(1), Is.EqualTo(5)); // not cached now
            Assert.That(memoized1(3), Is.EqualTo(3)); // but this one is cached
        }
    }
}
