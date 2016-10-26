#######
Logging
#######

General logging interface.

    .. class:: ILogger

        Logger abstraction, provides methods for ``Trace``, ``Debug``, ``Info``, ``Warn``, ``Error`` and ``Fatal`` logging output.

        * ``Trace`` - very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development.
        * ``Debug`` - debugging information, less detailed than trace, typically not enabled in production environment.
        * ``Info`` - information messages, which are normally enabled in production environment.
        * ``Warn`` - warning messages, typically for non-critical issues, which can be recovered or which are temporary failures.
        * ``Error`` - error messages - most of the time these are Exceptions.
        * ``Fatal`` - very serious errors!

    .. class:: ILoggerFactory

        The only method ``GetLogger`` must return ``ILogger`` implementation with specified logger name. To get current class name you can use ``Utils.GetCurrentClassLogger`` method. Example of registration with Autofac:

            .. code-block:: c#

                var loggerFactory = new Saritasa.Tools.NLog.LoggerFactory();
                builder.RegisterInstance(loggerFactory).AsImplementedInterfaces().SingleInstance();

    .. class:: LogManager

        Static class for logging. Use it if you do not want to pass ``ILoggerFactory`` to every service/handler. Before first use ``SetFactory`` should be called.
