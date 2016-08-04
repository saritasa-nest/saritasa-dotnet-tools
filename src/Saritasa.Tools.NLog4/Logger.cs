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
        private readonly global::NLog.ILogger logger;

        /// <summary>
        /// Initializes a new instance of the Logger class.
        /// </summary>
        /// <param name="logger">Instance of NLog logger.</param>
        public Logger(global::NLog.ILogger logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string Name => this.logger.Name;

        #region Debug

        /// <inheritdoc/>
        public bool IsDebugEnabled => this.logger.IsDebugEnabled;

        /// <inheritdoc/>
        public void Debug(string message) => this.logger.Debug(message);

        /// <inheritdoc/>
        public void Debug(string format, params object[] args) => this.logger.Debug(format, args);

        /// <inheritdoc/>
        public void Debug(IFormatProvider provider, string format, params object[] args) => this.logger.Debug(provider, format, args);

        /// <inheritdoc/>
        public void Debug(Exception exception, string message) => this.logger.Debug(exception, message);

        /// <inheritdoc/>
        public void Debug(Exception exception, string format, params object[] args) => this.logger.Debug(exception, format, args);

        /// <inheritdoc/>
        public void Debug(Exception exception, IFormatProvider provider, string format, params object[] args) => this.logger.Debug(exception, provider, format, args);

        #endregion

        #region Trace

        /// <inheritdoc/>
        public bool IsTraceEnabled => this.logger.IsTraceEnabled;

        /// <inheritdoc/>
        public void Trace(string message) => this.logger.Trace(message);

        /// <inheritdoc/>
        public void Trace(string format, params object[] args) => this.logger.Trace(format, args);

        /// <inheritdoc/>
        public void Trace(IFormatProvider provider, string format, params object[] args) => this.logger.Trace(provider, format, args);

        /// <inheritdoc/>
        public void Trace(Exception exception, string message) => this.logger.Trace(exception, message);

        /// <inheritdoc/>
        public void Trace(Exception exception, string format, params object[] args) => this.logger.Trace(exception, format, args);

        /// <inheritdoc/>
        public void Trace(Exception exception, IFormatProvider provider, string format, params object[] args) => this.logger.Trace(exception, provider, format, args);

        #endregion

        #region Info

        /// <inheritdoc/>
        public bool IsInfoEnabled => this.logger.IsInfoEnabled;

        /// <inheritdoc/>
        public void Info(string message) => this.logger.Info(message);

        /// <inheritdoc/>
        public void Info(string format, params object[] args) => this.logger.Info(format, args);

        /// <inheritdoc/>
        public void Info(IFormatProvider provider, string format, params object[] args) => this.logger.Info(provider, format, args);

        /// <inheritdoc/>
        public void Info(Exception exception, string message) => this.logger.Info(exception, message);

        /// <inheritdoc/>
        public void Info(Exception exception, string format, params object[] args) => this.logger.Info(exception, format, args);

        /// <inheritdoc/>
        public void Info(Exception exception, IFormatProvider provider, string format, params object[] args) => this.logger.Info(exception, provider, format, args);

        #endregion

        #region Warn

        /// <inheritdoc/>
        public bool IsWarnEnabled => this.logger.IsWarnEnabled;

        /// <inheritdoc/>
        public void Warn(string message) => this.logger.Warn(message);

        /// <inheritdoc/>
        public void Warn(string format, params object[] args) => this.logger.Warn(format, args);

        /// <inheritdoc/>
        public void Warn(IFormatProvider provider, string format, params object[] args) => this.logger.Warn(provider, format, args);

        /// <inheritdoc/>
        public void Warn(Exception exception, string message) => this.logger.Warn(exception, message);

        /// <inheritdoc/>
        public void Warn(Exception exception, string format, params object[] args) => this.logger.Warn(exception, format, args);

        /// <inheritdoc/>
        public void Warn(Exception exception, IFormatProvider provider, string format, params object[] args) => this.logger.Warn(exception, provider, format, args);

        #endregion

        #region Error

        /// <inheritdoc/>
        public bool IsErrorEnabled => this.logger.IsErrorEnabled;

        /// <inheritdoc/>
        public void Error(string message) => this.logger.Error(message);

        /// <inheritdoc/>
        public void Error(string format, params object[] args) => this.logger.Error(format, args);

        /// <inheritdoc/>
        public void Error(IFormatProvider provider, string format, params object[] args) => this.logger.Error(provider, format, args);

        /// <inheritdoc/>
        public void Error(Exception exception, string message) => this.logger.Error(exception, message);

        /// <inheritdoc/>
        public void Error(Exception exception, string format, params object[] args) => this.logger.Error(exception, format, args);

        /// <inheritdoc/>
        public void Error(Exception exception, IFormatProvider provider, string format, params object[] args) => this.logger.Error(exception, provider, format, args);

        #endregion

        #region Fatal

        /// <inheritdoc/>
        public bool IsFatalEnabled => this.logger.IsFatalEnabled;

        /// <inheritdoc/>
        public void Fatal(string message) => this.logger.Error(message);

        /// <inheritdoc/>
        public void Fatal(string format, params object[] args) => this.logger.Error(format, args);

        /// <inheritdoc/>
        public void Fatal(IFormatProvider provider, string format, params object[] args) => this.logger.Error(provider, format, args);

        /// <inheritdoc/>
        public void Fatal(Exception exception, string message) => this.logger.Error(exception, message);

        /// <inheritdoc/>
        public void Fatal(Exception exception, string format, params object[] args) => this.logger.Error(exception, format, args);

        /// <inheritdoc/>
        public void Fatal(Exception exception, IFormatProvider provider, string format, params object[] args) => this.logger.Error(exception, provider, format, args);

        #endregion
    }
}
