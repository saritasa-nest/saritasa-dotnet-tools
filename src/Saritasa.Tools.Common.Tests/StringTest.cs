// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Tests
{
    using System;
    using NUnit.Framework;
    using Extensions;
    using Utils;

    /// <summary>
    /// String utils tests.
    /// </summary>
    [TestFixture]
    public class StringTest
    {
        [Test]
        public void Snake_case_should_work()
        {
            Assert.That(StringUtils.ConvertToSnakeCase("MyTestMethod"), Is.EqualTo("My_Test_Method"));
        }

        [Test]
        public void Truncate_should_trim_string()
        {
            String str = "1234567890";
            String str2 = StringUtils.Truncate(str, 5);
            Assert.That(str2.Length, Is.EqualTo(5));
        }

        private enum TestEnum
        {
            ValueA,
            ValueB,
            ValueC,
        }

        [Test]
        public void Test_default_parse_calls()
        {
            Assert.False(StringUtils.ParseDefault("incorrect", false));
            Assert.That(StringUtils.ParseDefault("incorrect", 1), Is.EqualTo(1));
            Assert.That(StringUtils.ParseDefault("incorrect", 'a'), Is.EqualTo('a'));
            Assert.That(StringUtils.ParseDefault("incorrect", DateTime.MaxValue), Is.EqualTo(DateTime.MaxValue));
            Assert.That(StringUtils.ParseDefault("incorrect", 1.2), Is.EqualTo(1.2));
            Assert.That(StringUtils.ParseDefault("incorrect", TestEnum.ValueA), Is.EqualTo(TestEnum.ValueA));
            Assert.That(StringUtils.ParseDefault("ValueC", TestEnum.ValueA), Is.EqualTo(TestEnum.ValueC));
            Assert.That(StringUtils.ParseDefault("incorrect", 1.2f), Is.EqualTo(1.2f));
        }

        [Test]
        public void Safe_substring_should_not_throw_exceptions()
        {
            Assert.That(StringUtils.SafeSubstring("123", 1, 3), Is.EqualTo("23"));
            Assert.That(StringUtils.SafeSubstring("123", 1, 6), Is.EqualTo("23"));
        }
    }
}
