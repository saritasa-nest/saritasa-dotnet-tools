// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Specialized;
using Xunit;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Tests
{
    /// <summary>
    /// Dictionary and name value collection tests.
    /// </summary>
    public class DictionaryTests
    {
#if NET40 || NET452 || NET461 || NETSTANDARD2_0
        [Fact]
        public void Name_value_collection_get_default_value_should_get_default()
        {
            // Arrange
            var collection = new NameValueCollection
            {
                { "1", "abc" },
                { "2", "bca" }
            };

            // Act & assert
            Assert.Equal("default", DictionaryUtils.GetValueOrDefault(collection, "5", "default"));
            Assert.Equal("abc", DictionaryUtils.GetValueOrDefault(collection, "1", "abc"));
        }

        [Fact]
        public void Name_value_collection_get_default_values_should_get_default()
        {
            // Arrange
            var collection = new NameValueCollection
            {
                { "1", "abc" },
                { "1", "abcde" },
                { "2", "bca" }
            };

            // Act & assert
            Assert.Equal(new[] { "default" },
                DictionaryUtils.GetValuesOrDefault(collection, "5", new[] { "default" }));
            Assert.Equal(new[] { "abc", "abcde" },
                DictionaryUtils.GetValuesOrDefault(collection, "1", new[] { "abc" }));
        }
#endif
    }
}
