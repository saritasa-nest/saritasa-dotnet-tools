// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Internal
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <summary>
    /// The is to be used for internal logging purposes.
    /// </summary>
    public static class InternalLogger
    {
        private static readonly object LockObj = new object();

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
        private static readonly TraceSource TraceSource = new TraceSource("Saritasa.Tools");
#endif

        /// <summary>
        /// Write internal logs to log file.
        /// </summary>
        public static string LogFile { get; set; }

        /// <summary>
        /// Is internal logger enabled.
        /// </summary>
        public static bool IsEnabled { get; set; } = false;

        /// <summary>
        /// Log to standard diagnostic trace.
        /// </summary>
        public static bool LogToTrace { get; set; } = false;

        /// <summary>
        /// Log to console.
        /// </summary>
        public static bool LogToConsole { get; set; } = false;

        /// <summary>
        /// Min log level. Error by default.
        /// </summary>
        public static LogLevel MinLogLevel { get; set; } = LogLevel.Error;

        /// <summary>
        /// Log levels.
        /// </summary>
        public enum LogLevel
        {
            /// <summary>
            /// Trace.
            /// </summary>
            Trace,

            /// <summary>
            /// Debug.
            /// </summary>
            Debug,

            /// <summary>
            /// Info.
            /// </summary>
            Info,

            /// <summary>
            /// Warning.
            /// </summary>
            Warn,

            /// <summary>
            /// Error.
            /// </summary>
            Error,
        }

        /// <summary>
        /// Log trace.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="source">Optional message source. Can be class name.</param>
        public static void Trace(string message, string source = null)
        {
            Write(LogLevel.Trace, message, source);
        }

        /// <summary>
        /// Log debug.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="source">Optional message source. Can be class name.</param>
        public static void Debug(string message, string source = null)
        {
            Write(LogLevel.Debug, message, source);
        }

        /// <summary>
        /// Log info.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="source">Optional message source. Can be class name.</param>
        public static void Info(string message, string source = null)
        {
            Write(LogLevel.Info, message, source);
        }

        /// <summary>
        /// Log warning.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="source">Optional message source. Can be class name.</param>
        public static void Warn(string message, string source = null)
        {
            Write(LogLevel.Warn, message, source);
        }

        /// <summary>
        /// Log error.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="source">Optional message source. Can be class name.</param>
        public static void Error(string message, string source = null)
        {
            Write(LogLevel.Error, message, source);
        }

        private static void Write(LogLevel level, string message, string source = null)
        {
            if (IsEnabled == false)
            {
                return;
            }

            if (MinLogLevel > level)
            {
                return;
            }

            try
            {
                var sb = new StringBuilder(message.Length + source.Length + 36);
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture));
                sb.Append(" [");
                sb.Append(level.ToString().ToUpperInvariant());
                sb.Append("] ");
                if (source != null)
                {
                    sb.Append(source);
                    sb.Append(": ");
                }
                sb.AppendLine(message);

                if (string.IsNullOrEmpty(LogFile) == false)
                {
                    lock (LockObj)
                    {
                        File.AppendAllText(LogFile, sb.ToString());
                    }
                }

                if (LogToConsole)
                {
                    Console.Write(sb.ToString());
                }

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
                if (LogToTrace)
                {
                    TraceEventType eventType;
                    switch (level)
                    {
                        case LogLevel.Trace:
                        case LogLevel.Debug:
                            eventType = TraceEventType.Verbose;
                            break;
                        case LogLevel.Info:
                            eventType = TraceEventType.Information;
                            break;
                        case LogLevel.Warn:
                            eventType = TraceEventType.Warning;
                            break;
                        case LogLevel.Error:
                            eventType = TraceEventType.Error;
                            break;
                        default:
                            eventType = TraceEventType.Information;
                            break;
                    }
                    TraceSource.TraceEvent(eventType, 0, sb.ToString());
                }
#endif
            }
            catch (Exception)
            {
                // we have no place to log the message to so we ignore it
            }
        }

        /// <summary>
        /// Is trace logging enabled.
        /// </summary>
        public static bool IsTraceEnabled
        {
            get { return IsEnabled && MinLogLevel >= LogLevel.Trace; }
        }

        /// <summary>
        /// Is debug logging enabled.
        /// </summary>
        public static bool IsDebugEnabled
        {
            get { return IsEnabled && MinLogLevel >= LogLevel.Debug; }
        }

        /// <summary>
        /// Is info logging enabled.
        /// </summary>
        public static bool IsInfoEnabled
        {
            get { return IsEnabled && MinLogLevel >= LogLevel.Info; }
        }

        /// <summary>
        /// Is warning logging enabled.
        /// </summary>
        public static bool IsWarnEnabled
        {
            get { return IsEnabled && MinLogLevel >= LogLevel.Warn; }
        }

        /// <summary>
        /// Is error logging enabled.
        /// </summary>
        public static bool IsErrorEnabled
        {
            get { return IsEnabled && MinLogLevel >= LogLevel.Error; }
        }
    }
}
