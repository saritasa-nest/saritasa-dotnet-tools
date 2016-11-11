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
        static void Main()
        {
            BenchmarkRunner.Run<QueriesBenchmarks>();
        }
    }
}
