using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Saritasa.Tools.Benchmark
{
    class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">App args.</param>
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CommandsBenchmarks>();
        }
    }
}
