// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.NLog
{
    using Logging;

    /// <summary>
    /// Implementation of ILoggerFactory interface for NLog.
    /// </summary>
    public class LoggerFactory : ILoggerFactory
    {
        /// <inheritdoc/>
        public ILogger GetLogger(string name)
        {
            return new Logger(global::NLog.LogManager.GetLogger(name));
        }
    }
}
