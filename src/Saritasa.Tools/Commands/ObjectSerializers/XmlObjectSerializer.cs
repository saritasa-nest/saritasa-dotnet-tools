// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
namespace Saritasa.Tools.Commands.ObjectSerializers
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Serialize object using XmlSerializer.
    /// </summary>
    public class XmlObjectSerializer : IObjectSerializer
    {
        private static XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
        {
            CheckCharacters = false,
            Indent = false,
            OmitXmlDeclaration = false,
        };

        /// <inheritdoc />
        public object Deserialize(byte[] bytes, Type type)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(type);
            using (var stream = new MemoryStream(bytes))
            {
                return xmlSerializer.Deserialize(stream);
            }
        }

        /// <inheritdoc />
        public byte[] Serialize(object obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            using (var stream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                {
                    xmlSerializer.Serialize(xmlWriter, obj);
                    return stream.ToArray();
                }
            }
        }
    }
}
#endif
