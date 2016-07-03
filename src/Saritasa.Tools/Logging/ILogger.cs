// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Logging
{
    using System;

    /// <summary>
    /// Logger abstraction.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Gets this logger's name.
        /// </summary>
        string Name { get; }

        #region Debug

        /// <summary>
        /// Is the logger instance enabled for the DEBUG level?
        /// </summary>
        bool IsDebugEnabled { get; }

        /// <summary>
        /// Logs a message at the DEBUG level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Debug(string message);

        /// <summary>
        /// Logs a message at the DEBUG level according to the specified <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Debug(string format, params object[] args);

        /// <summary>
        /// Logs a message at the DEBUG level according to the specified <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> which provides
        /// culture-specific formatting capabilities.</param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Debug(IFormatProvider provider, string format, params object[] args);

        /// <summary>
        /// Logs an exception and an additional message at the DEBUG level.
        /// </summary>
        /// <param name="exception"> The exception to log.</param>
        /// <param name="message">Additional information regarding the
        /// logged exception.</param>
        void Debug(Exception exception, string message);

        /// <summary>
        /// Logs an exception and an additional message at the DEBUG level
        /// </summary>
        /// <param name="exception">The exception to log </param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Debug(Exception exception, string format, params object[] args);

        /// <summary>
        /// Logs an exception and an additional message at the DEBUG level
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/> which provides
        /// culture-specific formatting capabilities.</param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Debug(Exception exception, IFormatProvider provider, string format, params object[] args);

        #endregion

        #region Trace

        /// <summary>
        /// Is the logger instance enabled for the TRACE level?
        /// </summary>
        bool IsTraceEnabled { get; }

        /// <summary>
        /// Logs a message at the TRACE level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Trace(string message);

        /// <summary>
        /// Logs a message at the TRACE level according to the specified <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Trace(string format, params object[] args);

        /// <summary>
        /// Logs a message at the TRACE level according to the specified <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> which provides
        /// culture-specific formatting capabilities.</param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Trace(IFormatProvider provider, string format, params object[] args);

        /// <summary>
        /// Logs an exception and an additional message at the TRACE level.
        /// </summary>
        /// <param name="exception"> The exception to log.</param>
        /// <param name="message">Additional information regarding the
        /// logged exception.</param>
        void Trace(Exception exception, string message);

        /// <summary>
        /// Logs an exception and an additional message at the TRACE level
        /// </summary>
        /// <param name="exception">The exception to log </param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Trace(Exception exception, string format, params object[] args);

        /// <summary>
        /// Logs an exception and an additional message at the TRACE level
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/> which provides
        /// culture-specific formatting capabilities.</param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Trace(Exception exception, IFormatProvider provider, string format, params object[] args);

        #endregion

        #region Info

        /// <summary>
        /// Is the logger instance enabled for the INFO level?
        /// </summary>
        bool IsInfoEnabled { get; }

        /// <summary>
        /// Logs a message at the INFO level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Info(string message);

        /// <summary>
        /// Logs a message at the INFO level according to the specified <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// Logs a message at the INFO level according to the specified <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> which provides
        /// culture-specific formatting capabilities.</param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Info(IFormatProvider provider, string format, params object[] args);

        /// <summary>
        /// Logs an exception and an additional message at the INFO level.
        /// </summary>
        /// <param name="exception"> The exception to log.</param>
        /// <param name="message">Additional information regarding the
        /// logged exception.</param>
        void Info(Exception exception, string message);

        /// <summary>
        /// Logs an exception and an additional message at the INFO level
        /// </summary>
        /// <param name="exception">The exception to log </param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Info(Exception exception, string format, params object[] args);

        /// <summary>
        /// Logs an exception and an additional message at the INFO level
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/> which provides
        /// culture-specific formatting capabilities.</param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Info(Exception exception, IFormatProvider provider, string format, params object[] args);

        #endregion

        #region Warn

        /// <summary>
        /// Is the logger instance enabled for the WARN level?
        /// </summary>
        bool IsWarnEnabled { get; }

        /// <summary>
        /// Logs a message at the WARN level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Warn(string message);

        /// <summary>
        /// Logs a message at the WARN level according to the specified <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Warn(string format, params object[] args);

        /// <summary>
        /// Logs a message at the WARN level according to the specified <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> which provides
        /// culture-specific formatting capabilities.</param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Warn(IFormatProvider provider, string format, params object[] args);

        /// <summary>
        /// Logs an exception and an additional message at the WARN level.
        /// </summary>
        /// <param name="exception"> The exception to log.</param>
        /// <param name="message">Additional information regarding the
        /// logged exception.</param>
        void Warn(Exception exception, string message);

        /// <summary>
        /// Logs an exception and an additional message at the WARN level
        /// </summary>
        /// <param name="exception">The exception to log </param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Warn(Exception exception, string format, params object[] args);

        /// <summary>
        /// Logs an exception and an additional message at the WARN level
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/> which provides
        /// culture-specific formatting capabilities.</param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Warn(Exception exception, IFormatProvider provider, string format, params object[] args);

        #endregion

        #region Error

        /// <summary>
        /// Is the logger instance enabled for the ERROR level?
        /// </summary>
        bool IsErrorEnabled { get; }

        /// <summary>
        /// Logs a message at the ERROR level.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Error(string message);

        /// <summary>
        /// Logs a message at the ERROR level according to the specified <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Error(string format, params object[] args);

        /// <summary>
        /// Logs a message at the ERROR level according to the specified <paramref name="format"/> and <paramref name="args"/>.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> which provides
        /// culture-specific formatting capabilities.</param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Error(IFormatProvider provider, string format, params object[] args);

        /// <summary>
        /// Logs an exception and an additional message at the ERROR level.
        /// </summary>
        /// <param name="exception"> The exception to log.</param>
        /// <param name="message">Additional information regarding the
        /// logged exception.</param>
        void Error(Exception exception, string message);

        /// <summary>
        /// Logs an exception and an additional message at the ERROR level
        /// </summary>
        /// <param name="exception">The exception to log </param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Error(Exception exception, string format, params object[] args);

        /// <summary>
        /// Logs an exception and an additional message at the ERROR level
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="provider">An <see cref="IFormatProvider"/> which provides
        /// culture-specific formatting capabilities.</param>
        /// <param name="format">A composite format string that contains placeholders for the
        /// arguments.</param>
        /// <param name="args">An <see cref="object"/> array containing zero or more objects
        /// to format.</param>
        void Error(Exception exception, IFormatProvider provider, string format, params object[] args);

        #endregion
    }
}
