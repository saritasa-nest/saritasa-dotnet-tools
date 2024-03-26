// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Saritasa.Tools.Common.Utils;
#pragma warning disable CS1591

namespace Saritasa.Tools.Common.Tests;

/// <summary>
/// Flow, enumerable tests.
/// </summary>
public class FlowTests
{
    private event EventHandler<EventArgs>? EventArgsTestEvent;

    [Fact]
    public void RaiseAll_Subscribe3Delegates_3Raised()
    {
        // Arrange
        int a = 0;
        EventHandler<EventArgs> testDelegate = (sender, args) =>
        {
            a += 1;
        };
        EventArgsTestEvent = null;
        EventArgsTestEvent += testDelegate;
        EventArgsTestEvent += testDelegate;
        EventArgsTestEvent += testDelegate;

        // Act
        FlowUtils.RaiseAll(this, new EventArgs(), ref EventArgsTestEvent);

        // Assert
        Assert.Equal(3, a);
    }

    [Fact]
    public void RaiseAll_SubscribeOneDelegateWithException_AllDelegatesExecuted()
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
    public void RaiseAll_NullDelegate_NoException()
    {
        // Arrange
        EventArgsTestEvent = null;
        EventArgs eventArgs = new EventArgs();

        // Act
        FlowUtils.RaiseAll(this, eventArgs, ref EventArgsTestEvent);
    }

    /// <summary>
    /// Custom exception for tests only.
    /// </summary>
    private sealed class CustomException : Exception
    {
    }

    [Fact]
    public void Retry_SimpleDelegateWithFixedRetryStrategy_NoException()
    {
        // Arrange
        Func<int> customMethodReturn = () => 123;

        // Act
        FlowUtils.Retry(customMethodReturn, FlowUtils.CreateFixedDelayRetryStrategy());
        FlowUtils.Retry(customMethodReturn, FlowUtils.CreateFixedDelayRetryStrategy(int.MaxValue, TimeSpan.MaxValue));
    }

    [Fact]
    public void Retry_ExceptionsDelegateWithFixedRetryStrategyAndTransientExceptions_ThrowsOnCertainExceptions()
    {
        // Arrange
        Action customMethodReturnWithCustomException = () => throw new CustomException();

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
    public void Retry_FixedDelayStrategy_DelayMoreThan100Ms()
    {
        // Arrange
        Action customMethodReturnWithCustomException = () => throw new CustomException();
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        try
        {
            FlowUtils.Retry(customMethodReturnWithCustomException,
                FlowUtils.CreateFixedDelayRetryStrategy(3, TimeSpan.FromMilliseconds(50)), typeof(CustomException));
        }
        catch (CustomException)
        {
            // Suppress our specific exception.
        }
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds >= 100);
    }

    [Fact]
    public void Retry_FixedRetryStrategyWithFirstFailFast_ShouldFirstFailFast()
    {
        // Arrange
        Action customMethodReturnWithCustomException = () => throw new CustomException();
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
            // Suppress our specific exception.
        }
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds >= 100);
    }

    [Fact]
    public void Retry_CombinedLogHandler_LogWorks()
    {
        // Arrange
        Action customMethodReturnWithCustomException = () => throw new CustomException();
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
            // Suppress our specific exception.
        }

        // Assert
        Assert.Equal(2, totalAttempts);
    }

    [Fact]
    public void RetryAsync_FixedDelayStrategyAndExceptionsDelegate_ShouldWaitMoreThan100Ms()
    {
        // Arrange
        Func<Task<int>> customMethodReturnWithCustomExceptionAsync = () =>
            Task.Factory.StartNew<int>(() => throw new CustomException());
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
    public void Retry_FixedDelayRetryStrategyWithFirstFailFast_FirstFailFast()
    {
        // Arrange
        Func<Task<int>> customMethodReturnWithCustomExceptionAsync = () =>
            Task.Factory.StartNew<int>(() => throw new CustomException());
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
    public void RetryAsync_CombineLogHandlerStrategy_Logs()
    {
        // Arrange
        Func<Task<int>> customMethodReturnWithCustomExceptionAsync = () =>
            Task.Factory.StartNew<int>(() => throw new CustomException());
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
    public void RetryAsync_FixedDelayRetryStrategyWithFirst5ExceptionsReturn_TotalAttemptsMoreThan4()
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
    public void Memoize_TwoCallsWithTheSameKey_MethodsIsCalledOnce()
    {
        // Arrange
        int value = 0;
        var memoized1 = FlowUtils.Memoize(() => ++value);

        // Act & Assert
        Assert.Equal(1, memoized1());
        Assert.Equal(1, memoized1());
    }

    [Fact]
    public void Memoize_MaxAgeCacheStrategy_CacheIsExpired()
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
    public void Memoize_MaxCountCacheStrategy_CacheClearedCorrectly()
    {
        // Arrange
        int value = 0;
        var memoized1 = FlowUtils.Memoize(
            a => ++value,
            FlowUtils.CreateMaxCountCacheStrategy<int, int>(maxCount: 3, removeCount: 2)
        );

        // Act & assert
        Assert.Equal(1, memoized1(1));
        Assert.Equal(2, memoized1(2));
        Assert.Equal(3, memoized1(3));
        Assert.Equal(1, memoized1(1)); // Cached here.
        Assert.Equal(4, memoized1(4)); // Cache reset here.
        Assert.Equal(5, memoized1(1)); // Not cached now.
        Assert.Equal(3, memoized1(3)); // But this one is cached.
    }

    [Fact]
    public void Memoize_MaxCountCacheStrategyWithCompositeKey_WorkExpectedForKeyPair()
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
