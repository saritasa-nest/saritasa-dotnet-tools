using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Queries;
using BenchmarkDotNet.Attributes;

namespace Saritasa.Tools.Benchmark
{
    /// <summary>
    /// Query pipeline benchmarks.
    /// </summary>
    public class QueriesBenchmarks
    {
        const int NumberOfInterations = 250;

        [QueryHandlers]
        public sealed class MathQueryHandlers
        {
            public decimal Sum(decimal a, decimal b)
            {
                return a + b;
            }
        }

        [Benchmark(Baseline = true)]
        public decimal RunQueryDirect()
        {
            decimal result = 0;
            for (int i = 0; i < NumberOfInterations; i++)
            {
                result += new MathQueryHandlers().Sum(2, 3);
            }
            return result;
        }

        [Benchmark]
        public decimal RunQueryWithPipeline()
        {
            decimal result = 0;
            var queryPipeline = QueryPipeline.CreateDefaultPipeline(QueryPipeline.NullResolver);
            for (int i = 0; i < NumberOfInterations; i++)
            {
                result += queryPipeline.Query<MathQueryHandlers>().With(q => q.Sum(2, 3));
            }
            return result;
        }
    }
}
