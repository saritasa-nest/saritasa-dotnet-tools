// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Saritasa.Tools.Common.Extensions;

namespace Saritasa.Tools.Common.Tests
{
    /// <summary>
    /// All extension methods tests.
    /// </summary>
    public class ExtensionsTests
    {
        [Fact]
        public void AddOrUpdate_Dictionary_GetDefaultValueShouldGetDefault()
        {
            // Arrange
            IDictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(1, "abc");
            dict.Add(2, "bca");

            // Act & assert
            Assert.Equal("default", dict.GetValueOrDefault(5, "default"));
            Assert.Equal("abc", dict.GetValueOrDefault(1, "abc"));
        }

        [Fact]
        public void AddOrUpdate_Dictionary_AddOrUpdateShouldReturnNewValue()
        {
            // Arrange
            IDictionary<int, int> dict = new Dictionary<int, int>();
            dict[1] = 10;

            // Act
            dict.AddOrUpdate(0, (key, value) => ++value, 10);
            dict.AddOrUpdate(1, (key, value) => ++value);

            // Assert
            Assert.Equal(11, dict[0]);
            Assert.Equal(11, dict[1]);
        }

        [Fact]
        public void ChunkSelectRange_ListWithItems_ShouldIterateWholeList()
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
        public void ChunkSelect_ListWithItems_ShouldIterateWholeList()
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
    }
}
