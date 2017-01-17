// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Tests
{
    using System;
    using Xunit;
    using Utils;

    /// <summary>
    /// String utils tests.
    /// </summary>
    public class StringTest
    {
        [Theory]
        [InlineData("My_Test_Method", "MyTestMethod")]
        [InlineData("The_Big_Bang", "TheBigBang")]
        public void Snake_case_should_work(string expect, string target)
        {
            Assert.Equal(expect, StringUtils.ConvertToSnakeCase(target));
        }

        [Theory]
        [InlineData("1234567890", 5, 5)]
        [InlineData("1234567890", 15, 10)]
        public void Truncate_should_trim_string(string target, int truncate, int expectedLength)
        {
            var str2 = StringUtils.Truncate(target, truncate);
            Assert.Equal(expectedLength, str2.Length);
        }

        private enum TestEnum
        {
            ValueA,
            ValueB,
            ValueC,
        }

        [Theory]
        [InlineData("incorrect", false, false, false)]
        [InlineData("true", true, false, true)]
        [InlineData("t", false, true, true)]
        [InlineData("no", true, true, false)]
        public void Test_default_parse_calls_for_bools(string val, bool @default, bool extended, bool expect)
        {
            Assert.Equal(expect, StringUtils.ParseDefault(val, @default, extended));
        }

        [Fact]
        public void Test_default_parse_calls()
        {
            Assert.Equal(1, StringUtils.ParseDefault("incorrect", 1)); // int
            Assert.Equal('a', StringUtils.ParseDefault("incorrect", 'a')); // char
            Assert.Equal(DateTime.MaxValue, StringUtils.ParseDefault("incorrect", DateTime.MaxValue)); // datetime
            Assert.Equal(1.2, StringUtils.ParseDefault("incorrect", 1.2)); // double
            Assert.Equal(TestEnum.ValueA, StringUtils.ParseDefault("incorrect", TestEnum.ValueA)); // enum
            Assert.Equal(TestEnum.ValueC, StringUtils.ParseDefault("ValueC", TestEnum.ValueA)); // enum
            Assert.Equal(1.2f, StringUtils.ParseDefault("incorrect", 1.2f)); // float
        }

        [Theory]
        [InlineData("23", "123", 1, 3)]
        [InlineData("23", "123", 1, 6)]
        public void Safe_substring_should_not_throw_exceptions(string expect, string target, int start, int count)
        {
            Assert.Equal(expect, StringUtils.SafeSubstring(target, start, count));
        }
    }
}
