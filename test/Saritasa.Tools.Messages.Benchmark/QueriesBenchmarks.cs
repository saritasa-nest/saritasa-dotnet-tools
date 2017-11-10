using System;
using BenchmarkDotNet.Attributes;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Common;
using Saritasa.Tools.Messages.Queries;

namespace Saritasa.Tools.Messages.Benchmark
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
            var pipelineService = new DefaultMessagePipelineService();
            pipelineService.PipelineContainer.AddQueryPipeline()
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryObjectResolverMiddleware
                {
                    UseInternalObjectResolver = true
                })
                .AddMiddleware(new Queries.PipelineMiddlewares.QueryExecutorMiddleware());
            decimal result = 0;

            for (int i = 0; i < NumberOfInterations; i++)
            {
                result += pipelineService.Query<MathQueryHandlers>().With(q => q.Sum(2, 3));
            }
            return result;
        }
    }
}
