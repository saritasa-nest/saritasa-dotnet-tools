// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Saritasa.Tools.Messages.Internal
{
    /// <summary>
    /// The is to be used for internal logging purposes.
    /// </summary>
    public static class InternalLogger
    {
        private static readonly object lockObj = new object();

        /// <summary>
        /// Write internal logs to log file.
        /// </summary>
        public static string LogFile { get; set; }

        /// <summary>
        /// Is internal logger enabled.
        /// </summary>
        public static bool IsEnabled { get; set; }

        /// <summary>
        /// Log to standard diagnostic trace.
        /// </summary>
        public static bool LogToTrace { get; set; }

        /// <summary>
        /// Log to console.
        /// </summary>
        public static bool LogToConsole { get; set; }

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
            Error
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
            if (!IsEnabled)
            {
                return;
            }
            if (MinLogLevel > level)
            {
                return;
            }
            if (source == null)
            {
                source = string.Empty;
            }

            try
            {
                var sb = new StringBuilder(message.Length + source.Length + 36);
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff", CultureInfo.InvariantCulture));
                sb.Append(" [");
                sb.Append(level.ToString().ToUpperInvariant());
                sb.Append("] ");
                sb.Append(source);
                sb.Append(": ");
                sb.AppendLine(message);

                if (!string.IsNullOrEmpty(LogFile))
                {
                    lock (lockObj)
                    {
                        File.AppendAllText(LogFile, sb.ToString());
                    }
                }

                if (LogToConsole)
                {
                    Console.Write(sb.ToString());
                }

#if NET452
                if (LogToTrace)
                {
                    System.Diagnostics.Trace.WriteLine(sb.ToString());
                }
#endif
            }
            catch (Exception)
            {
                // We have no place to log the message to so we ignore it.
            }
        }

        /// <summary>
        /// Is trace logging enabled.
        /// </summary>
        public static bool IsTraceEnabled => IsEnabled && MinLogLevel <= LogLevel.Trace;

        /// <summary>
        /// Is debug logging enabled.
        /// </summary>
        public static bool IsDebugEnabled => IsEnabled && MinLogLevel <= LogLevel.Debug;

        /// <summary>
        /// Is info logging enabled.
        /// </summary>
        public static bool IsInfoEnabled => IsEnabled && MinLogLevel <= LogLevel.Info;

        /// <summary>
        /// Is warning logging enabled.
        /// </summary>
        public static bool IsWarnEnabled => IsEnabled && MinLogLevel <= LogLevel.Warn;

        /// <summary>
        /// Is error logging enabled.
        /// </summary>
        public static bool IsErrorEnabled => IsEnabled && MinLogLevel <= LogLevel.Error;
    }
}
