#######
Logging
#######

General logging interface.

.. class:: ILogger

    Logger abstraction, provides methods for ``Debug``, ``Trace``, ``Info``, ``Warn``, ``Error`` and ``Fatal`` logging output.

.. class:: ILoggerFactory

    The only method ``GetLogger`` must return ``ILogger`` implementation with specified logger name. To get current class name you can use ``Utils.GetCurrentClassLogger`` method.

Example of registration with Autofac:

    .. code-block:: c#

            var loggerFactory = new Saritasa.Tools.NLog.LoggerFactory();
            builder.RegisterInstance(loggerFactory).AsImplementedInterfaces().SingleInstance();
