// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Tests
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Xunit;
    using Domain.Exceptions;

    /// <summary>
    /// Domain tests.
    /// </summary>
    public class DomainTests
    {
        [Fact]
        public void Domain_exception_should_serialize_deserialize_correctly()
        {
            var domainException = new DomainException("Test");
            var formatter = new BinaryFormatter();

            DomainException deserializedDomainException = null;
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, domainException);
                memoryStream.Seek(0, SeekOrigin.Begin);
                deserializedDomainException = (DomainException)formatter.Deserialize(memoryStream);
            }

            Assert.Equal(domainException.Message, deserializedDomainException.Message);
        }
    }
}
