// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if NET452
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Configuration
{
    /// <summary>
    /// Saritasa Tools Messages configuration section handler class for configuring from App.config .
    /// </summary>
    public sealed class XmlConfigSectionHandler : IConfigurationSectionHandler
    {
        /// <summary>
        /// Creates a configuration section handler.
        /// </summary>
        /// <param name="parent">Parent object.</param>
        /// <param name="configContext">Configuration context object.</param>
        /// <param name="section">Section XML node.</param>
        /// <returns>The created section handler object.</returns>
        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            return ParsePipeline(section);
        }

        private IMessagePipelineContainer ParsePipeline(XmlNode node)
        {
            var pipelines = new List<IMessagePipeline>();
            foreach (XmlNode childNode in node)
            {
                if (childNode.Attributes == null)
                {
                    continue;
                }
                var typeAttr = childNode.Attributes["type"];
                if (typeAttr == null)
                {
                    continue;
                }
                var type = Type.GetType(typeAttr.Value, throwOnError: false);
                if (type == null)
                {
                    throw new MessagesConfigurationException($"Cannot find type {typeAttr.Value}.");
                }
                var pipeline = Activator.CreateInstance(type) as IMessagePipeline;
                if (pipeline == null)
                {
                    throw new MessagesConfigurationException($"Cannot instaniate pipeline from type {type.Name}. Make sure " +
                                                             $"it has public parameterless ctor and implements IMessagePipeline interface.");
                }
                ParseMiddlewares(pipeline, childNode.ChildNodes);
                pipelines.Add(pipeline);
            }
            return new SimpleMessagePipelineContainer(pipelines);
        }

        private void ParseMiddlewares(IMessagePipeline pipeline, XmlNodeList nodes)
        {
            foreach (XmlNode node in nodes)
            {
                var parameters = new Dictionary<string, string>();
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                if (node.Attributes == null)
                {
                    throw new MessagesConfigurationException("Middleware tag does not have attributes.");
                }
                foreach (XmlAttribute attr in node.Attributes)
                {
                    parameters.Add(attr.Name, attr.Value);
                }

                var typeName = node.Attributes["type"].Value;
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    throw new MessagesConfigurationException("Middleware tag does not have \"type\" attribute.");
                }

                var type = Type.GetType(typeName);
                if (type == null)
                {
                    throw new MessagesConfigurationException($"Cannot load type {typeName}.");
                }

                var ctor = type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 1
                    && c.GetParameters()[0].ParameterType == typeof(IDictionary<string, string>));
                if (ctor == null)
                {
                    ctor = type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
                }

                if (ctor == null)
                {
                    var msg = "Cannot find public parameterless constructor or constructor that accepts IDictionary<string, string>.";
                    throw new MessagesConfigurationException(msg);
                }

                var middleware = ctor.GetParameters().Length == 1
                    ? ctor.Invoke(new object[] { parameters }) as IMessagePipelineMiddleware
                    : ctor.Invoke(new object[] { }) as IMessagePipelineMiddleware;
                if (middleware == null)
                {
                    throw new MessagesConfigurationException($"Cannot instaniate pipeline middleware {type.Name}.");
                }
                pipeline.AddMiddlewares(middleware);
            }
        }
    }
}
#endif
