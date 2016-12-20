Logging
=======

For logging we rely on Microsoft common logging abstractions:

https://github.com/aspnet/Logging

Package ``Saritasa.Tools.NLog4`` contains implementation for NLog library. Here is an example of usage:

.. code-block:: c#

    ILoggerProvider loggerProvider = new NLogLoggerProvider();
    var logger = loggerProvider.CreateLogger("test");

    // simple
    logger.LogTrace("trace");
    logger.LogDebug("debug");
    logger.LogInformation("info");
    logger.LogError("error");
    logger.LogWarning("warning");
    logger.LogCritical("critical");

    // scopes
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

Output:

    ::

        00:17:12 Trace trace
        00:17:12 Debug debug
        00:17:12 Info info
        00:17:12 Error error
        00:17:12 Warn warning
        00:17:12 Fatal critical
        00:17:12 Info => (scope 1)
        test 1
        00:17:12 Info => (scope 1) => (scope 2)
        log2: test 2
        00:17:12 Info => (scope 1) => (scope 2) => (scope 3)
        log1: test 2
        00:17:12 Info => (scope 1) => (scope 2) => (scope 3)
        log2: test 2
        00:17:12 Info => (scope 1)
        test 3