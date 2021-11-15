// Copyright (c) 2015-2021, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

        [Fact]
        public void PagedListEmpty_NoInput_EmptyPagedList()
        {
            // Arrange & Act
            var pagedResult = PagedListFactory.Empty<object>();

            // Assert
            Assert.Equal(0, pagedResult.TotalCount);
            Assert.Equal(0, pagedResult.TotalPages);
            Assert.False(pagedResult.Any());
        }

        [Fact]
        public void OffsetLimitEmpty_NoInput_EmptyOffsetLimitList()
        {
            // Arrange & Act
            var offsetLimitResult = OffsetLimitListFactory.Empty<object>();

            // Assert
            Assert.Equal(0, offsetLimitResult.TotalCount);
            Assert.False(offsetLimitResult.Any());
        }

        [Fact]
        public void TotalCountEmpty_NoInput_EmptyTotalCountList()
        {
            // Arrange & Act
            var totalCountResult = TotalCountListFactory.Empty<object>();

            // Assert
            Assert.Equal(0, totalCountResult.TotalCount);
            Assert.False(totalCountResult.Any());
        }

        [Fact]
        public void BinaryFormatterSerialize_PagedListWithDate_PersistAfterDeserialize()
        {
            // Arrange
            var pagedList = new PagedList<int>(new[] { 10 }, 1, 10, 1);
            var formatter = new BinaryFormatter();
            PagedList<int> deserializedPagedList = null;

            // Act
#pragma warning disable SYSLIB0011 // Type or member is obsolete
            using var memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, pagedList);
            memoryStream.Seek(0, SeekOrigin.Begin);
            deserializedPagedList = (PagedList<int>)formatter.Deserialize(memoryStream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

            // Assert
            Assert.Equal(pagedList.Page, deserializedPagedList.Page);
            Assert.Equal(pagedList.PageSize, deserializedPagedList.PageSize);
            Assert.Equal(pagedList.TotalCount, deserializedPagedList.TotalCount);
            Assert.Equal(pagedList.Count(), deserializedPagedList.Count());
            Assert.Equal(pagedList[0], deserializedPagedList[0]);
        }

#if NET5_0_OR_GREATER
        [Fact]
        public void JsonSerialize_PagedListMetadataWithDate_PersistAfterDeserialize()
        {
            // Arrange
            var pagedListMetadata = new PagedList<int>(new[] { 10 }, 1, 10, 1).ToMetadataObject();

            // Act
            var json = System.Text.Json.JsonSerializer.Serialize(pagedListMetadata);
            var deserializedPagedListMetadata = System.Text.Json.JsonSerializer.Deserialize<PagedListMetadataDto<int>>(json);

            // Assert
            Assert.Equal(pagedListMetadata.Metadata.Page, deserializedPagedListMetadata.Metadata.Page);
            Assert.Equal(pagedListMetadata.Metadata.PageSize, deserializedPagedListMetadata.Metadata.PageSize);
            Assert.Equal(pagedListMetadata.Metadata.TotalCount, deserializedPagedListMetadata.Metadata.TotalCount);
            Assert.Equal(pagedListMetadata.Items.Count(), deserializedPagedListMetadata.Items.Count());
            Assert.Equal(pagedListMetadata.Items.First(), deserializedPagedListMetadata.Items.First());
        }

        [Fact]
        public void JsonSerialize_OffsetLimitListMetadataWithDate_PersistAfterDeserialize()
        {
            // Arrange
            var offsetLimitListMetadata = new OffsetLimitList<int>(new[] { 10 }, 1, 10, 1).ToMetadataObject();

            // Act
            var json = System.Text.Json.JsonSerializer.Serialize(offsetLimitListMetadata);
            var deserializedOffsetLimitListMetadata = System.Text.Json.JsonSerializer.Deserialize<OffsetLimitMetadataDto<int>>(json);

            // Assert
            Assert.Equal(offsetLimitListMetadata.Metadata.Limit, deserializedOffsetLimitListMetadata.Metadata.Limit);
            Assert.Equal(offsetLimitListMetadata.Metadata.Offset, deserializedOffsetLimitListMetadata.Metadata.Offset);
            Assert.Equal(offsetLimitListMetadata.Metadata.TotalCount, deserializedOffsetLimitListMetadata.Metadata.TotalCount);
            Assert.Equal(offsetLimitListMetadata.Items.Count(), deserializedOffsetLimitListMetadata.Items.Count());
            Assert.Equal(offsetLimitListMetadata.Items.First(), deserializedOffsetLimitListMetadata.Items.First());
        }
#endif
    }
}
