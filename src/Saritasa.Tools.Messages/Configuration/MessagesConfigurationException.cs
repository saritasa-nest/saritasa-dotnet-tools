// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Configuration
{
    /// <summary>
    /// Saritasa Tools messages configuration exception.
    /// </summary>
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class MessagesConfigurationException : Exception
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public MessagesConfigurationException()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public MessagesConfigurationException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        public MessagesConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
