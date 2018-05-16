using System;
using System.Linq;
using BenchmarkDotNet.Running;

namespace Saritasa.Tools.Messages.Benchmark
{
    /// <summary>
    /// Program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Saritasa.Tools.Messages.Benchmark.exe [TypeName].");
                return;
            }
            var typeName = args[0];
            var type = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(t => t.Name == typeName);
            if (type == null)
            {
                Console.WriteLine("Cannot find benchmark class.");
                return;
            }
            BenchmarkRunner.Run(type);
        }
    }
}
