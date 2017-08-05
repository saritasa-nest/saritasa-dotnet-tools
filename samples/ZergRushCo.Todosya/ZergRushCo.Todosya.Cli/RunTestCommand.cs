using System;
using System.Linq;
using Saritasa.Tools.Messages.TestRuns;
using ZergRushCo.Todosya.Domain.IntegrationTests;

namespace ZergRushCo.Todosya.Cli
{
    public class RunTestCommand
    {
        private readonly ITestRunLogger logger = new ConsoleTestRunLogger();

        public TestRunResult Run(string file)
        {
            var loader = new Saritasa.Tools.Messages.TestRuns.Loaders.FilesLoader(file);
            var testRun = loader.Get().First();
            var testRunRunner = new AppTestRunRunner();
            var result = testRunRunner.Run(testRun, logger);
            result.Dump(logger);
            return result;
        }
    }
}
