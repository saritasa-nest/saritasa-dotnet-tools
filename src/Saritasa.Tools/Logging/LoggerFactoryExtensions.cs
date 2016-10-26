using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Logging
{
    /// <summary>
    /// Extensions for logger factory
    /// </summary>
    public static class LoggerFactoryExtensions
    {
        /// <summary>
        /// Checking up type of logger
        /// </summary>
        public static bool IsLoggerFactorySupportScope(this ILoggerFactory @this, string name)
            => @this.GetLogger(name) is IScopedLogger;

        /// <summary>
        /// Creating scoped logger which support scope.
        /// </summary>
        public static IScopedLogger CreateScoped(this ILoggerFactory @this, string name) 
            => @this.GetLogger(name) as IScopedLogger;
    }
}
