// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6 && !NET40
using System;
using System.Collections.Generic;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Configuration
{
    /// <summary>
    /// Class to configure messages module from XML file.
    /// </summary>
    public class XmlConfiguration
    {
        /// <summary>
        /// Gets the default list of <see cref="IMessagePipeline" /> objects by parsing
        /// the application configuration file (<c>app.exe.config</c>).
        /// </summary>
        public static IEnumerable<IMessagePipeline> AppConfig
        {
            get
            {
                object o = System.Configuration.ConfigurationManager.GetSection("pipelines");
                return o as IEnumerable<IMessagePipeline>;
            }
        }
    }
}
#endif
