using BenchmarkDotNet.Running;

namespace Saritasa.Tools.Messages.Benchmark
{
    /// <summary>
    /// Program.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">App args.</param>
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<QueriesBenchmarks>();
        }
    }
}
