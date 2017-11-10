// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Xunit;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Tests
{
    /// <summary>
    /// Object tests.
    /// </summary>
    public class ObjectTests
    {
        public class ReferenceType
        {
        }

        public struct ValueType
        {
        }

        [Fact]
        public void Creating_reference_type_delegate_should_work()
        {
            // Arrange
            var factory = ObjectUtils.CreateTypeFactory<ReferenceType>();

            // Act & Assert
            Assert.IsType<ReferenceType>(factory());
        }

        [Fact]
        public void Creating_value_type_delegate_should_work()
        {
            // Arrange
            var factory = ObjectUtils.CreateTypeFactory<ValueType>();

            // Act & Assert
            Assert.IsType<ValueType>(factory());
        }
    }
}
