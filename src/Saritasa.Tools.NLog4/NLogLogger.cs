// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.NLog4
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Wrap NLog's Logger in a Microsoft.Extensions.Logging's interface <see cref="Microsoft.Extensions.Logging.ILogger" />.
    /// </summary>
    internal class NLogLogger : ILogger
    {
        private readonly NLog.Logger logger;

        public NLogLogger(NLog.Logger logger)
        {
            this.logger = logger;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var nLogLogLevel = ConvertLogLevel(logLevel);
            if (IsEnabled(nLogLogLevel))
            {
                if (formatter == null)
                {
                    throw new ArgumentNullException(nameof(formatter));
                }
                var message = formatter(state, exception);

                if (!string.IsNullOrEmpty(message))
                {
                    // message arguments are not needed as it is already checked that the loglevel is enabled.
                    var eventInfo = NLog.LogEventInfo.Create(nLogLogLevel, logger.Name, message);
                    eventInfo.Exception = exception;
                    eventInfo.Properties["EventId.Id"] = eventId.Id;
                    eventInfo.Properties["EventId.Name"] = eventId.Name;
                    eventInfo.Properties["EventId"] = eventId;
                    logger.Log(eventInfo);
                }
            }
        }

        /// <summary>
        /// Is logging enabled for this logger at this <paramref name="logLevel" />?
        /// </summary>
        /// <param name="logLevel">Log level.</param>
        /// <returns>True if provided log level enabled.</returns>
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            var convertLogLevel = ConvertLogLevel(logLevel);
            return logger.IsEnabled(convertLogLevel);
        }

        /// <summary>
        /// Is logging enabled for this logger at this <paramref name="logLevel" />?
        /// </summary>
        private bool IsEnabled(NLog.LogLevel logLevel)
        {
            return logger.IsEnabled(logLevel);
        }

        /// <summary>
        /// Converts loglevel to NLog variant.
        /// </summary>
        /// <param name="logLevel">Level to be converted.</param>
        /// <returns>Converted level.</returns>
        private static NLog.LogLevel ConvertLogLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return NLog.LogLevel.Trace;
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return NLog.LogLevel.Debug;
                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return NLog.LogLevel.Info;
                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return NLog.LogLevel.Warn;
                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return NLog.LogLevel.Error;
                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return NLog.LogLevel.Fatal;
                case Microsoft.Extensions.Logging.LogLevel.None:
                    return NLog.LogLevel.Off;
                default:
                    return NLog.LogLevel.Debug;
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            return NLog.NestedDiagnosticsContext.Push(state);
        }
    }
}
