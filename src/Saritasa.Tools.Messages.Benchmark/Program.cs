using System.Runtime.CompilerServices;
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
        //[MethodImpl(MethodImplOptions.NoOptimization)]
        static void Main()
        {
            //BenchmarkRunner.Run<ExpressionExecutorBenchmarks>();

            BenchmarkRunner.Run<QueriesBenchmarks>();
            //var queriesBenchmarks = new QueriesBenchmarks();
            //queriesBenchmarks.RunQueryWithPipeline();
            //var bench = new ExpressionExecutorBenchmarks();
            //bench.RunCompiledExpressionExecuteWrapperForNonGenericSum();
        }
    }
}
