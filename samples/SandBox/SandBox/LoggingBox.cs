using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace SandBox
{
    /// <summary>
    /// Logging sandbox.
    /// </summary>
    public static class LoggingBox
    {
        public static void Test()
        {
            ILoggerProvider loggerProvider = new NLogLoggerProvider();
            var logger = loggerProvider.CreateLogger("test");

            // Simple.
            logger.LogTrace("trace");
            logger.LogDebug("debug");
            logger.LogInformation("info");
            logger.LogError("error");
            logger.LogWarning("warning");
            logger.LogCritical("critical");

            // Scopes.
            using (logger.BeginScope("(scope 1)"))
            {
                logger.LogInformation("test 1");
                using (logger.BeginScope("(scope 2)"))
                {
                    var logger2 = loggerProvider.CreateLogger("test");
                    logger2.LogInformation("log2: test 2");
                    using (logger2.BeginScope("(scope 3)"))
                    {
                        logger.LogInformation("log1: test 2");
                        logger2.LogInformation("log2: test 2");
                    }
                }
                logger.LogInformation("test 3");
            }
        }
    }
}
