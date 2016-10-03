// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.NLog
{
    using System;
    using Logging;

    /// <summary>
    /// Implementation of ILogger interface for NLog.
    /// </summary>
    public class Logger : ILogger
    {
        readonly global::NLog.ILogger logger;

        /// <summary>
        /// Initializes a new instance of the Logger class.
        /// </summary>
        /// <param name="logger">Instance of NLog logger.</param>
        public Logger(global::NLog.ILogger logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string Name => logger.Name;

        #region Debug

        /// <inheritdoc/>
        public bool IsDebugEnabled => logger.IsDebugEnabled;

        /// <inheritdoc/>
        public void Debug(string message) => logger.Debug(message);

        /// <inheritdoc/>
        public void Debug(string format, params object[] args) => logger.Debug(format, args);

        /// <inheritdoc/>
        public void Debug(IFormatProvider provider, string format, params object[] args) => logger.Debug(provider, format, args);

        /// <inheritdoc/>
        public void Debug(Exception exception, string message) => logger.Debug(exception, message);

        /// <inheritdoc/>
        public void Debug(Exception exception, string format, params object[] args) => logger.Debug(exception, format, args);

        /// <inheritdoc/>
        public void Debug(Exception exception, IFormatProvider provider, string format, params object[] args) => logger.Debug(exception, provider, format, args);

        #endregion

        #region Trace

        /// <inheritdoc/>
        public bool IsTraceEnabled => logger.IsTraceEnabled;

        /// <inheritdoc/>
        public void Trace(string message) => logger.Trace(message);

        /// <inheritdoc/>
        public void Trace(string format, params object[] args) => logger.Trace(format, args);

        /// <inheritdoc/>
        public void Trace(IFormatProvider provider, string format, params object[] args) => logger.Trace(provider, format, args);

        /// <inheritdoc/>
        public void Trace(Exception exception, string message) => logger.Trace(exception, message);

        /// <inheritdoc/>
        public void Trace(Exception exception, string format, params object[] args) => this.logger.Trace(exception, format, args);

        /// <inheritdoc/>
        public void Trace(Exception exception, IFormatProvider provider, string format, params object[] args) => this.logger.Trace(exception, provider, format, args);

        #endregion

        #region Info

        /// <inheritdoc/>
        public bool IsInfoEnabled => logger.IsInfoEnabled;

        /// <inheritdoc/>
        public void Info(string message) => logger.Info(message);

        /// <inheritdoc/>
        public void Info(string format, params object[] args) => logger.Info(format, args);

        /// <inheritdoc/>
        public void Info(IFormatProvider provider, string format, params object[] args) => logger.Info(provider, format, args);

        /// <inheritdoc/>
        public void Info(Exception exception, string message) => logger.Info(exception, message);

        /// <inheritdoc/>
        public void Info(Exception exception, string format, params object[] args) => logger.Info(exception, format, args);

        /// <inheritdoc/>
        public void Info(Exception exception, IFormatProvider provider, string format, params object[] args) => logger.Info(exception, provider, format, args);

        #endregion

        #region Warn

        /// <inheritdoc/>
        public bool IsWarnEnabled => logger.IsWarnEnabled;

        /// <inheritdoc/>
        public void Warn(string message) => logger.Warn(message);

        /// <inheritdoc/>
        public void Warn(string format, params object[] args) => logger.Warn(format, args);

        /// <inheritdoc/>
        public void Warn(IFormatProvider provider, string format, params object[] args) => logger.Warn(provider, format, args);

        /// <inheritdoc/>
        public void Warn(Exception exception, string message) => logger.Warn(exception, message);

        /// <inheritdoc/>
        public void Warn(Exception exception, string format, params object[] args) => logger.Warn(exception, format, args);

        /// <inheritdoc/>
        public void Warn(Exception exception, IFormatProvider provider, string format, params object[] args) => logger.Warn(exception, provider, format, args);

        #endregion

        #region Error

        /// <inheritdoc/>
        public bool IsErrorEnabled => logger.IsErrorEnabled;

        /// <inheritdoc/>
        public void Error(string message) => logger.Error(message);

        /// <inheritdoc/>
        public void Error(string format, params object[] args) => logger.Error(format, args);

        /// <inheritdoc/>
        public void Error(IFormatProvider provider, string format, params object[] args) => logger.Error(provider, format, args);

        /// <inheritdoc/>
        public void Error(Exception exception, string message) => logger.Error(exception, message);

        /// <inheritdoc/>
        public void Error(Exception exception, string format, params object[] args) => logger.Error(exception, format, args);

        /// <inheritdoc/>
        public void Error(Exception exception, IFormatProvider provider, string format, params object[] args) => logger.Error(exception, provider, format, args);

        #endregion

        #region Fatal

        /// <inheritdoc/>
        public bool IsFatalEnabled => logger.IsFatalEnabled;

        /// <inheritdoc/>
        public void Fatal(string message) => logger.Error(message);

        /// <inheritdoc/>
        public void Fatal(string format, params object[] args) => logger.Error(format, args);

        /// <inheritdoc/>
        public void Fatal(IFormatProvider provider, string format, params object[] args) => logger.Error(provider, format, args);

        /// <inheritdoc/>
        public void Fatal(Exception exception, string message) => logger.Error(exception, message);

        /// <inheritdoc/>
        public void Fatal(Exception exception, string format, params object[] args) => logger.Error(exception, format, args);

        /// <inheritdoc/>
        public void Fatal(Exception exception, IFormatProvider provider, string format, params object[] args) => logger.Error(exception, provider, format, args);

        #endregion
    }
}
