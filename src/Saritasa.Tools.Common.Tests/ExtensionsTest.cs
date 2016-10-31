// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Common.Tests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Extensions;

    /// <summary>
    /// All extension methods tests.
    /// </summary>
    [TestFixture]
    public class ExtensionsTest
    {
        [Test]
        public void String_format_should_format()
        {
            Assert.That("{0} + {1} = {2}".FormatWith(2, 2, 4), Is.EqualTo("2 + 2 = 4"));
        }

        [Test]
        public void Dictionary_get_default_value_should_get_default()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();
            dict.Add(1, "abc");
            dict.Add(2, "bca");
            Assert.That(dict.GetValueDefault(5, "default"), Is.EqualTo("default"));
            Assert.That(dict.GetValueDefault(1, "abc"), Is.EqualTo("abc"));
        }

        [Test]
        public void Chunk_select_range_should_return_subsets()
        {
            int capacity = 250;
            int sum = 0;
            IList<int> list = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }
            foreach (var sublist in CollectionsExtensions.ChunkSelectRange(list, 45))
            {
                foreach (var item in sublist)
                {
                    sum += item;
                }
            }

            Assert.That(sum, Is.EqualTo(31125));
        }

        [Test]
        public void Chunk_select_should_return_subsets()
        {
            int capacity = 250;
            int sum = 0;
            IList<int> list = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }
            foreach (var item in CollectionsExtensions.ChunkSelect(list, 45))
            {
                sum += item;
            }

            Assert.That(sum, Is.EqualTo(31125));
        }
    }
}
