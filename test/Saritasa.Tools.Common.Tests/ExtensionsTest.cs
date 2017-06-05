// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Saritasa.Tools.Common.Extensions;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Tests
{
    /// <summary>
    /// All extension methods tests.
    /// </summary>
    public class ExtensionsTest
    {
        [Fact]
        public void String_format_should_format()
        {
            // Arrange & act
            var result = "{0} + {1} = {2}".FormatWith(2, 2, 4);

            // Assert
            Assert.Equal("2 + 2 = 4", result);
        }

        [Fact]
        public void Dictionary_get_default_value_should_get_default()
        {
            // Arrange
            Dictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(1, "abc");
            dict.Add(2, "bca");

            // Act & assert
            Assert.Equal("default", dict.GetValueOrDefault(5, "default"));
            Assert.Equal("abc", dict.GetValueOrDefault(1, "abc"));
        }

        [Fact]
        public void Chunk_select_range_should_return_subsets()
        {
            // Arrange
            int capacity = 250;
            int sum = 0;
            IList<int> list = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }

            // Act
            foreach (var sublist in list.AsQueryable().ChunkSelectRange(45))
            {
                foreach (var item in sublist)
                {
                    sum += item;
                }
            }

            // Assert
            Assert.Equal(31125, sum);
        }

        [Fact]
        public void Chunk_select_should_return_subsets()
        {
            // Arrange
            int capacity = 250;
            int sum = 0;
            IList<int> list = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }

            // Act
            foreach (var item in list.AsQueryable().ChunkSelect(45))
            {
                sum += item;
            }

            // Assert
            Assert.Equal(31125, sum);
        }


        public class TestAttribute : Attribute { }

        public enum TestEnum
        {
            [Test]
            A,

            B
        }

        [Fact]
        public void Get_enum_value_attribute_should_return_attribute()
        {
            // Arrange
            var val = TestEnum.A;

            // Act
            var attr = EnumUtils.GetAttribute<TestAttribute>(val);

            // Arrange
            Assert.NotNull(attr);
            Assert.IsType<TestAttribute>(attr);
        }

        [Fact]
        public void Get_enum_value_attribute_should_return_null_for_empty_value()
        {
            // Arrange
            var val = TestEnum.B;

            // Act
            var attr = EnumUtils.GetAttribute<TestAttribute>(val);

            // Arrange
            Assert.Null(attr);
        }

        [Fact]
        public void Get_enum_value_attribute_should_return_null_for_incorrect_type()
        {
            // Arrange
            var val = TestEnum.A;

            // Act
            var attr = EnumUtils.GetAttribute<ObsoleteAttribute>(val);

            // Arrange
            Assert.Null(attr);
        }
    }
}
