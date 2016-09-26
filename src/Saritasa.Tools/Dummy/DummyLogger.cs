// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Dummy
{
    using System;
    using Logging;

    /// <summary>
    /// Empty logger implementation. Can be used for testing.
    /// </summary>
    public class DummyLogger : ILogger
    {
        /// <inheritdoc />
        public bool IsDebugEnabled => false;

        /// <inheritdoc />
        public bool IsErrorEnabled => false;

        /// <inheritdoc />
        public bool IsInfoEnabled => false;

        /// <inheritdoc />
        public bool IsTraceEnabled => false;

        /// <inheritdoc />
        public bool IsWarnEnabled => false;

        /// <inheritdoc />
        public bool IsFatalEnabled => false;

        /// <inheritdoc />
        public string Name => "Dummy";

        /// <inheritdoc />
        public void Debug(string message)
        {
        }

        /// <inheritdoc />
        public void Debug(Exception exception, string message)
        {
        }

        /// <inheritdoc />
        public void Debug(string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Debug(Exception exception, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Debug(IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Debug(Exception exception, IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Error(string message)
        {
        }

        /// <inheritdoc />
        public void Error(Exception exception, string message)
        {
        }

        /// <inheritdoc />
        public void Error(string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Error(Exception exception, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Error(IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Error(Exception exception, IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Info(string message)
        {
        }

        /// <inheritdoc />
        public void Info(Exception exception, string message)
        {
        }

        /// <inheritdoc />
        public void Info(string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Info(Exception exception, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Info(IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Info(Exception exception, IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Trace(string message)
        {
        }

        /// <inheritdoc />
        public void Trace(Exception exception, string message)
        {
        }

        /// <inheritdoc />
        public void Trace(string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Trace(Exception exception, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Trace(IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Trace(Exception exception, IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Warn(string message)
        {
        }

        /// <inheritdoc />
        public void Warn(Exception exception, string message)
        {
        }

        /// <inheritdoc />
        public void Warn(string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Warn(Exception exception, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Warn(IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Warn(Exception exception, IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Fatal(string message)
        {
        }

        /// <inheritdoc />
        public void Fatal(Exception exception, string message)
        {
        }

        /// <inheritdoc />
        public void Fatal(string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Fatal(Exception exception, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Fatal(IFormatProvider provider, string format, params object[] args)
        {
        }

        /// <inheritdoc />
        public void Fatal(Exception exception, IFormatProvider provider, string format, params object[] args)
        {
        }
    }
}
