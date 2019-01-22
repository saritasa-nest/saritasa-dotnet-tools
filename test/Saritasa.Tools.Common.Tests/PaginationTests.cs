// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Saritasa.Tools.Common.Pagination;

namespace Saritasa.Tools.Common.Tests
{
    /// <summary>
    /// Pagination tests.
    /// </summary>
    public class PaginationTests
    {
        [Fact]
        public void FromSource_ListOfInts_CorrectPagedData()
        {
            // Arrange
            int capacity = 250;
            IList<int> list = new List<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                list.Add(i);
            }

            // Act
            var pagedList = PagedListFactory.FromSource(list, 10, 10);
            var pagedList2 = PagedListFactory.FromSource(list, 10, 10);
            var pagedList3 = PagedListFactory.FromSource(list, 13, 25);
            var pagedList4 = PagedListFactory.FromSource(list, 20, 13);

            // Assert
            Assert.Equal(25, pagedList.TotalPages);
            Assert.Equal(10, pagedList2.Count());
            Assert.Equal(10, pagedList3.TotalPages);
            Assert.Equal(13, pagedList3.Page);
            Assert.Equal(20, pagedList4.TotalPages);
            Assert.Equal(3, pagedList4.Count());
        }

        [Fact]
        public void FromSource_ListOfIntsWithIntMaxPageSize_CorrectPagedData()
        {
            // Arrange
            var sourceData = new List<int>
            {
                1,
                2
            };

            // Act
            var pagedResult = PagedListFactory.FromSource(sourceData, 1, int.MaxValue);

            // Assert
            Assert.Equal(1, pagedResult.TotalPages);
            Assert.Equal(int.MaxValue, pagedResult.Limit);
            Assert.Equal(2, pagedResult.TotalCount);
            Assert.True(pagedResult.IsFirstPage);
            Assert.True(pagedResult.IsLastPage);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(int.MaxValue, pagedResult.PageSize);
        }
    }
}
