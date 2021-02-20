// Copyright (c) 2015-2021, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Running;

namespace Saritasa.Tools.Benchmarks
{
    /// <summary>
    /// Program class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Program arguments.</param>
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<CollectionUtilsDiffBenchmark>();
        }
    }
}
