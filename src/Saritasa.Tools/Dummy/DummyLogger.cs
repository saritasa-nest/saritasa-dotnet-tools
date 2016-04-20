//
// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
//

namespace Saritasa.Tools.Dummy
{
    using System;
    using Interfaces;

    /// <summary>
    /// Empty logger implementation. Can be used for testing.
    /// </summary>
    public class DummyLogger : ILogger
    {
        /// <inheritdoc />
        public bool IsDebugEnabled
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool IsErrorEnabled
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool IsInfoEnabled
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool IsTraceEnabled
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool IsWarnEnabled
        {
            get { return false; }
        }

        /// <inheritdoc />
        public string Name
        {
            get { return "Dummy"; }
        }

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
    }
}
