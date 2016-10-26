
using System;

namespace Saritasa.Tools.Logging
{
    /// <summary>
    /// Scoped logger for track scope of logging.
    /// </summary>
    public interface IScopedLogger : ILogger
    {
        /// <summary>
        /// State object of logger scope
        /// </summary>
        IScope Scope { get; }

        /// <summary>
        /// Starting entry point of scope.
        /// </summary>
        IDisposable BeginScope(string scopeName = null);
    }
}
