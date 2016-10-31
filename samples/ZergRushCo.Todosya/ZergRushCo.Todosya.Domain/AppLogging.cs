using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ZergRushCo.Todosya.Domain
{
    /// <summary>
    /// Application logger static class.
    /// </summary>
    public static class AppLogging
    {
        /// <summary>
        /// Empty scope without logic.
        /// </summary>
        public class NullScope : IDisposable
        {
            /// <inheritdoc />
            public void Dispose()
            {
            }
        }

        /// <summary>
        /// Empty logger.
        /// </summary>
        public class NullLogger : ILogger
        {
            /// <inheritdoc />
            public IDisposable BeginScope<TState>(TState state)
            {
                return new NullScope();
            }

            /// <inheritdoc />
            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            /// <inheritdoc />
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
            }
        }

        /// <summary>
        /// Logger factory that produces empty loggers.
        /// </summary>
        public class NullLoggerFactory : ILoggerFactory
        {
            /// <inheritdoc />
            public void AddProvider(ILoggerProvider provider)
            {
            }

            /// <inheritdoc />
            public ILogger CreateLogger(string categoryName)
            {
                return new NullLogger();
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }
        }

        /// <summary>
        /// Logger factory.
        /// </summary>
        public static ILoggerFactory LoggerFactory { get; } = new NullLoggerFactory();

        /// <summary>
        /// Creates logger from logger factory.
        /// </summary>
        /// <typeparam name="T">Type used by logger name.</typeparam>
        /// <returns>Logger.</returns>
        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    }
}
