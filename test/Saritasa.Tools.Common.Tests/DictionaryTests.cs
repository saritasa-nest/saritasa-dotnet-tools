// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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

        [Fact]
        public void AddOrUpdate_Dictionary_GetDefaultValueShouldGetDefault()
        {
            // Arrange
            IDictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(1, "abc");
            dict.Add(2, "bca");

            // Act & assert
            Assert.Equal("default", DictionaryUtils.GetValueOrDefault(dict, 5, "default"));
            Assert.Equal("abc", DictionaryUtils.GetValueOrDefault(dict, 1, "abc"));
        }

        [Fact]
        public void AddOrUpdate_Dictionary_AddOrUpdateShouldReturnNewValue()
        {
            // Arrange
            IDictionary<int, int> dict = new Dictionary<int, int>();
            dict[1] = 10;

            // Act
            DictionaryUtils.AddOrUpdate(dict, 0, (key, value) => ++value, 10);
            DictionaryUtils.AddOrUpdate(dict, 1, (key, value) => ++value);

            // Assert
            Assert.Equal(11, dict[0]);
            Assert.Equal(11, dict[1]);
        }
    }
}
