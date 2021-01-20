// Copyright (c) 2015-2021, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Xunit;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Tests
{
    /// <summary>
    /// String utils tests.
    /// </summary>
    public class StringTests
    {
        [Theory]
        [InlineData("1234567890", 5, 5)]
        [InlineData("1234567890", 15, 10)]
        [InlineData("", 4, 0)]
        public void SafeTruncate_NotTrimmedString_TrimmedString(string target, int truncate, int expectedLength)
        {
            // Arrange & Act
            var str = StringUtils.SafeTruncate(target, truncate);

            // Assert
            Assert.Equal(expectedLength, str.Length);
        }

        private enum TestEnum
        {
            ValueA,
            ValueB,
            ValueC,
        }

        [Theory]
        [InlineData("true", true, true)]
        [InlineData("t", false, true)]
        [InlineData("no", true, false)]
        public void ParseOrDefaultExtended_UseExtendedBool_UseExtendedValues(string val, bool @default, bool expect)
        {
            Assert.Equal(expect, StringUtils.ParseOrDefaultExtended(val, @default));
        }

        [Theory]
        [InlineData("incorrect", false, false)]
        [InlineData(null, true, true)]
        public void ParseOrDefaultExtended_UseExtendedBool_ReturnDefaultValue(string val, bool @default, bool expect)
        {
            Assert.Equal(expect, StringUtils.ParseOrDefaultExtended(val, @default));
        }

        [Fact]
        public void ParseOrDefault_CallWithIncorrectString_ShouldReturnDefaultValue()
        {
            Assert.Equal(1, StringUtils.ParseOrDefault("incorrect", 1)); // int
            Assert.Equal('a', StringUtils.ParseOrDefault("incorrect", 'a')); // char
            Assert.Equal(DateTime.MaxValue, StringUtils.ParseOrDefault("incorrect", DateTime.MaxValue)); // datetime
            Assert.Equal(1.2, StringUtils.ParseOrDefault("incorrect", 1.2)); // double
            Assert.Equal(TestEnum.ValueA, StringUtils.ParseOrDefault("incorrect", TestEnum.ValueA)); // enum
            Assert.Equal(TestEnum.ValueC, StringUtils.ParseOrDefault("ValueC", TestEnum.ValueA)); // enum
            Assert.Equal(1.2f, StringUtils.ParseOrDefault("incorrect", 1.2f)); // float
            Assert.True(StringUtils.ParseOrDefault("true", false)); // bool
            Assert.True(StringUtils.ParseOrDefault("incorrect", true)); // bool
            Assert.True(StringUtils.ParseOrDefault(null, true)); // bool
        }

        [Theory]
        [InlineData("23", "123", 1, 3)]
        [InlineData("23", "123", 1, 6)]
        public void SafeSubstring_ShortString_ShouldNotThrowExceptions(string expect, string target, int start, int count)
        {
            Assert.Equal(expect, StringUtils.SafeSubstring(target, start, count));
        }

        [Fact]
        public void JoinIgnoreEmpty_SpaceAsSeparator_AllowSpaceAsSeparator()
        {
            // Arrange
            var arr = new[] { "1", "2", "", "3" };

            // Act
            var result = StringUtils.JoinIgnoreEmpty(" ", arr);

            // Assert
            Assert.Equal("1 2 3", result);
        }

        [Fact]
        public void Capitalize_NotCapitalizedString_CapitalizedString()
        {
            // Arrange
            var target = "form";

            // Act
            var result = StringUtils.Capitalize(target);

            // Assert
            Assert.Equal("Form", result);
        }
    }
}
