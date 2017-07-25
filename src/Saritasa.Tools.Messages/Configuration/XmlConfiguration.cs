// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if NET452
using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Configuration
{
    /// <summary>
    /// Class to configure messages module from XML file.
    /// </summary>
    public class XmlConfiguration
    {
        /// <summary>
        /// Gets the <see cref="IMessagePipelineContainer" /> object by parsing
        /// the application configuration file (<c>app.exe.config</c>).
        /// </summary>
        public static IMessagePipelineContainer AppConfig
        {
            get
            {
                object o = System.Configuration.ConfigurationManager.GetSection("pipelines");
                return o as IMessagePipelineContainer;
            }
        }
    }
}
#endif
