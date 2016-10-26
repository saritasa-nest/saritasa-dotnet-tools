using Saritasa.Tools.Logging.Inetrnal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Logging
{
    /// <inheritdoc />
    public class DummyScopedLogger : IScopedLogger
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

        /// <inheritdoc />
        public IScope Scope { get; private set; }

        /// <inheritdoc />
        public IDisposable BeginScope(string scopeName = null)
        {
            if (Scope != null && !Scope.IsDisposed)
            {
                throw new InvalidOperationException("Scope already defined!");
            }

            Scope = new Scope(scopeName);

            return Scope;
        }
    }
}
