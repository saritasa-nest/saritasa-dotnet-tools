// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Tests
{
    using System;
    using Xunit;
    using Utils;

    /// <summary>
    /// Validation module test.
    /// </summary>
    public class GuardTests
    {
        [Fact]
        public void Valid_emails_should_not_throw_exception()
        {
            Guard.IsNotInvalidEmail("fwd2ivan@yandex.ru", "test");
            Guard.IsNotInvalidEmail("fwd2ivan+label@yandex.ru", "test");
            Guard.IsNotInvalidEmail("2fwd2ivan@yandex.ru", "test");
            Guard.IsNotInvalidEmail("ivan+ivan@kras.saritas.local", "test");
        }

        [Fact]
        public void Invalid_emails_should_throw_exception()
        {
            Assert.Throws<ArgumentException>(() => { Guard.IsNotInvalidEmail("fwd2ivanyandex.ru", "test"); });
            Assert.Throws<ArgumentException>(() => { Guard.IsNotInvalidEmail("2fwd2ivan@yandex", "test"); });
            Assert.Throws<ArgumentException>(() => { Guard.IsNotInvalidEmail("@yandex.ru", "test"); });
            Assert.Throws<ArgumentException>(() => { Guard.IsNotInvalidEmail("fwd2ivan@", "test"); });
        }

        [Fact]
        public void Is_not_null_should_throw_exception()
        {
            object obj = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                Guard.IsNotNull(obj, "obj");
            });
        }
    }
}
