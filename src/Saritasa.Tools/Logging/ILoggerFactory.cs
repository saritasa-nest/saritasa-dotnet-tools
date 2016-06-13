// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Logging
{
    /// <summary>
    /// <see cref="ILoggerFactory" /> instances manufacture <see cref="ILogger" />
    /// instances by name. These factory methods may create new instances
    /// or retrieve cached / pooled instances depending on the the
    /// name of the requested logger.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Returns an appropriate <see cref="ILogger"/> instance as specified by the name parameter.
        /// </summary>
        /// <param name="name">The name of the logger to return.</param>
        ILogger GetLogger(string name);
    }
}
