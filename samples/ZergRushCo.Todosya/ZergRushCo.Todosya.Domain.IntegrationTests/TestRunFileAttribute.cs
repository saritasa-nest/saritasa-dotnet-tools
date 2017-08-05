using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;
using Saritasa.Tools.Messages.TestRuns;

namespace ZergRushCo.Todosya.Domain.IntegrationTests
{
    /// <summary>
    /// Loads JSON data from embedded resource and prepares <see cref="TestRun" /> instances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class TestRunFileAttribute : DataAttribute
    {
        private readonly string resource;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resource">Resource name, for example
        /// "Reports.JobsMetricsTests.Crm_jobs_metrics_billable_nonbillable_test".</param>
        public TestRunFileAttribute(string resource)
        {
            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentNullException(nameof(resource));
            }
            this.resource = resource + ".json";
        }

        /// <inheritdoc />
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream($@"ZergRushCo.Todosya.Domain.IntegrationTests.{resource}"))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    yield return new object[]
                    {
                        TestRun.Load(streamReader)
                };
                }
            }
        }
    }
}
