// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.NLog
{
    using JetBrains.Annotations;

    /// <summary>
    /// Provider logger for NLog.
    /// </summary>
    public class NLogLoggerProvider : Microsoft.Extensions.Logging.ILoggerProvider
    {
        /// <summary>
        /// Create a logger with the name <paramref name="name" />.
        /// </summary>
        /// <param name="name">Name of the logger to be created.</param>
        /// <returns>New logger.</returns>
        public Microsoft.Extensions.Logging.ILogger CreateLogger([NotNull] string name)
        {
            return new NLogLogger(global::NLog.LogManager.GetLogger(name));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Nothing to implement so far.
        }
    }
}
