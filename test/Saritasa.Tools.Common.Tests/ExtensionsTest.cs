// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Tests
{
    using System.Collections.Generic;
    using Xunit;
    using Extensions;

    /// <summary>
    /// All extension methods tests.
    /// </summary>
    public class ExtensionsTest
    {
        [Fact]
        public void String_format_should_format()
        {
            Assert.Equal("2 + 2 = 4", "{0} + {1} = {2}".FormatWith(2, 2, 4));
        }

        [Fact]
        public void Dictionary_get_default_value_should_get_default()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(1, "abc");
            dict.Add(2, "bca");
            Assert.Equal("default", dict.GetValueDefault(5, "default"));
            Assert.Equal("abc", dict.GetValueDefault(1, "abc"));
        }

        [Fact]
        public void Chunk_select_range_should_return_subsets()
        {
            int capacity = 250;
            int sum = 0;
            IList<int> list = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }
            foreach (var sublist in list.ChunkSelectRange(45))
            {
                foreach (var item in sublist)
                {
                    sum += item;
                }
            }

            Assert.Equal(31125, sum);
        }

        [Fact]
        public void Chunk_select_should_return_subsets()
        {
            int capacity = 250;
            int sum = 0;
            IList<int> list = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }
            foreach (var item in list.ChunkSelect(45))
            {
                sum += item;
            }

            Assert.Equal(31125, sum);
        }
    }
}
