// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using NUnit.Framework;
    using Exceptions;

    [TestFixture]
    public class DomainTests
    {
        [Test]
        public void Domain_exception_should_serialize_deserialize_correctly()
        {
            var domainException = new DomainException("Test");
            var formatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, domainException);
                memoryStream.Seek(0, SeekOrigin.Begin);
                formatter.Deserialize(memoryStream);
            }
        }
    }
}
