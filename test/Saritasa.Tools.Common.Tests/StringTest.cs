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
        [Fact]
        public void Snake_case_should_work()
        {
            Assert.Equal("My_Test_Method", StringUtils.ConvertToSnakeCase("MyTestMethod"));
        }

        [Fact]
        public void Truncate_should_trim_string()
        {
            var str = "1234567890";
            var str2 = StringUtils.Truncate(str, 5);
            Assert.Equal(5, str2.Length);
        }

        private enum TestEnum
        {
            ValueA,
            ValueB,
            ValueC,
        }

        [Fact]
        public void Test_default_parse_calls()
        {
            Assert.False(StringUtils.ParseDefault("incorrect", false));
            Assert.True(StringUtils.ParseDefault("true", true));
            Assert.True(StringUtils.ParseDefault("t", false, true));
            Assert.False(StringUtils.ParseDefault("no", true, true));
            Assert.Equal(1, StringUtils.ParseDefault("incorrect", 1));
            Assert.Equal('a', StringUtils.ParseDefault("incorrect", 'a'));
            Assert.Equal(DateTime.MaxValue, StringUtils.ParseDefault("incorrect", DateTime.MaxValue));
            Assert.Equal(1.2, StringUtils.ParseDefault("incorrect", 1.2));
            Assert.Equal(TestEnum.ValueA, StringUtils.ParseDefault("incorrect", TestEnum.ValueA));
            Assert.Equal(TestEnum.ValueC, StringUtils.ParseDefault("ValueC", TestEnum.ValueA));
            Assert.Equal(1.2f, StringUtils.ParseDefault("incorrect", 1.2f));
        }

        [Fact]
        public void Safe_substring_should_not_throw_exceptions()
        {
            Assert.Equal("23", StringUtils.SafeSubstring("123", 1, 3));
            Assert.Equal("23", StringUtils.SafeSubstring("123", 1, 6));
        }
    }
}
