using System;
using System.Diagnostics;
using Saritasa.Tools.Common.Utils;
using Saritasa.Tools.Domain.Exceptions;

namespace SandBox
{
    /// <summary>
    /// Retry strategy demo.
    /// </summary>
    public static class RetryStrategyDemo
    {
        private static int maxNumberOfTries = 10;

        private static readonly Stopwatch delayStopwatch = new Stopwatch();
        private static readonly Stopwatch totalStopwatch = new Stopwatch();
        private static int attemptNumber = 0;

        private static readonly FlowUtils.RetryStrategy fixedDelay = FlowUtils.CreateFixedDelayRetryStrategy(
            numberOfTries: maxNumberOfTries,
            delay: TimeSpan.FromSeconds(2),
            firstFastRetry: false);

        private static readonly FlowUtils.RetryStrategy incrementalDelay = FlowUtils.CreateIncrementDelayRetryStrategy(
            numberOfTries: maxNumberOfTries,
            delay: TimeSpan.FromSeconds(2),
            increment: TimeSpan.FromSeconds(1),
            firstFastRetry: false);

        private static readonly FlowUtils.RetryStrategy expDelay = FlowUtils.CreateExponentialBackoffDelayRetryStrategy(
            numberOfTries: maxNumberOfTries,
            minBackoff: TimeSpan.FromSeconds(2),
            maxBackoff: TimeSpan.FromSeconds(35),
            deltaBackoff: null
        );

        private static readonly FlowUtils.RetryStrategy expDelayNormalized = FlowUtils.CreateExponentialBackoffNormalizedDelayRetryStrategy(
            numberOfTries: maxNumberOfTries,
            minBackoff: TimeSpan.FromSeconds(2),
            maxBackoff: TimeSpan.FromSeconds(35)
        );

        private static void MethodWithException()
        {
            attemptNumber++;
            Console.WriteLine($@"{attemptNumber:D2}: {delayStopwatch.Elapsed:mm\:ss\.ff}  {totalStopwatch.Elapsed:mm\:ss\.ff}");
            delayStopwatch.Reset();
            delayStopwatch.Start();
            throw new DomainException("Test");
        }

        public static void Run()
        {
            Console.WriteLine("#   delay     total time");
            try
            {
                // ST
                totalStopwatch.Start();
                Saritasa.Tools.Common.Utils.FlowUtils.Retry(
                    MethodWithException,
                    expDelayNormalized
                );
            }
            catch (Exception)
            {
            }
            delayStopwatch.Stop();
            totalStopwatch.Stop();
        }
    }
}
