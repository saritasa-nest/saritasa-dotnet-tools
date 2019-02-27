using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SandBox
{
    /// <summary>
    /// Test Memoize from Saritasa.Tools.Common in multi-thread mode.
    /// </summary>
    public class MemoizeStressTest
    {
        private const int TotalRunTimeSecs = 120;

        private const int Multiplier = 100;

        private const int TotalThreads = 10;

        private static readonly object @lock = new object();

        /// <summary>
        /// Test function that sums a and b.
        /// </summary>
        private static int GetInts(int a, int b)
        {
            Thread.Sleep(4 * Multiplier);
            Console.WriteLine($"[GetInts] thread: {Thread.CurrentThread.ManagedThreadId}, eq: {a} + {b}");
            return a + b;
        }

        /// <summary>
        /// Run test.
        /// </summary>
        public static void Test()
        {
            var memoizedGetInts = Saritasa.Tools.Common.Utils.FlowUtils.Memoize(
                new Func<int, int, int>(GetInts),
                Saritasa.Tools.Common.Utils.FlowUtils.CreateMaxCountCacheStrategy<int, int, int>(maxCount: 30, removeCount: 5, purge: false)
            );

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var tasks = new List<Task>();

            for (int i = 0; i < TotalThreads; i++)
            {
                var task = Task.Run(() =>
                {
                    var localStopwatch = new Stopwatch();
                    var a = 0;
                    var b = 0;
                    var elapsedSeconds = 0;
                    while (elapsedSeconds < TotalRunTimeSecs)
                    {
                        localStopwatch.Start();
                        var res = memoizedGetInts(a, b);
                        Debug.Assert(a + b == res, "Incorrect result!");
                        localStopwatch.Stop();
                        Console.WriteLine($"[Test] thread: {Thread.CurrentThread.ManagedThreadId}, eq: {a} + {b} = {res}, time: {localStopwatch.ElapsedMilliseconds} ms");
                        localStopwatch.Reset();
                        a++;
                        b++;
                        lock (@lock)
                        {
                            elapsedSeconds = (int) stopwatch.Elapsed.TotalSeconds;
                        }
                    }
                });
                tasks.Add(task);
                Thread.Sleep(15 * Multiplier);
            }

            Task.WaitAll(tasks.ToArray());
            stopwatch.Stop();
        }
    }
}
