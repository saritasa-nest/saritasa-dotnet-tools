// Copyright (c) 2015-2021, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Tests
{
    /// <summary>
    /// Collection utils.
    /// </summary>
    public class CollectionTests
    {
        [DebuggerDisplay("{Id}, {Name}")]
        private sealed class User
        {
            public int Id { get; }

            public string Name { get; internal set; }

            public User(int id, string name)
            {
                this.Id = id;
                this.Name = name;
            }
        }

        private sealed class UserIdentityEqualityComparer : IEqualityComparer<User>
        {
            public bool Equals(User x, User y) => x.Id == y.Id;

            public int GetHashCode(User obj) => obj.Id.GetHashCode();
        }

        private sealed class UserEqualityComparer : IEqualityComparer<User>
        {
            public bool Equals(User x, User y) => y != null && x != null && x.Id == y.Id && x.Name == y.Name;

            public int GetHashCode(User obj) => obj.Id.GetHashCode() ^ obj.Name.GetHashCode();
        }

        [Fact]
        public void Diff_TargetCollectionWithNewElements_NewElementsAreAdded()
        {
            // Arrange
            var source = new[] { 1, 2, 3 };
            var target = new[] { 1, 2, 4 };

            // Act
            var diff = CollectionUtils.Diff(source, target);
            var diffEq = CollectionUtils.Diff(source, target, EqualityComparer<int>.Default);

            // Assert
            Assert.Equal(new[] { 4 }, diff.Added);
            Assert.Equal(new[] { 4 }, diffEq.Added);
        }

        [Fact]
        public void Diff_TargetCollectionWithoutOldElements_OldElementsAreRemoved()
        {
            // Arrange
            var source = new[] { 1, 2, 3 };
            var target = new[] { 1, 4 };

            // Act
            var diff = CollectionUtils.Diff(source, target);
            var diffEq = CollectionUtils.Diff(source, target, EqualityComparer<int>.Default);

            // Assert
            Assert.Equal(new[] { 2, 3 }, diff.Removed);
            Assert.Equal(new[] { 2, 3 }, diffEq.Removed);
        }

        [Fact]
        public void Diff_TargetCollectionWithExistingElements_ExistingElementsAreUpdated()
        {
            // Arrange
            var doug = new User(1, "Doug");
            var source = new[]
            {
                doug,
                new User(2, "Pasha")
            };
            var target = new[]
            {
                doug,
                new User(2, "Pavel")
            };

            // Act
            var diff = CollectionUtils.Diff(source, target, (u1, u2) => u1.Id == u2.Id);
            var diffEq = CollectionUtils.Diff(source, target, new UserIdentityEqualityComparer());

            // Assert
            Assert.Equal(new[] { new DiffResultUpdatedItems<User>(source[1], target[1]) }, diff.Updated);
            Assert.Equal(new[] { new DiffResultUpdatedItems<User>(source[1], target[1]) }, diffEq.Updated);
        }

        [Fact]
        public void Diff_TargetCollectionWithUpdatedElementsAndComparer_NotUpdatedElementsSkipped()
        {
            // Arrange
            var source = new User[]
            {
                new(1, "Doug"),
                new(2, "Pasha")
            };
            var target = new User[]
            {
                new(1, "Doug"),
                new(2, "Pavel")
            };

            // Act
            var diff = CollectionUtils.Diff(source, target, (u1, u2) => u1.Id == u2.Id, (u1, u2) =>
                u2 != null && (u1 != null && (u1.Id == u2.Id && u1.Name == u2.Name)));
            var diffEq = CollectionUtils.Diff(source, target, new UserIdentityEqualityComparer(), (u1, u2) =>
                u2 != null && (u1 != null && (u1.Id == u2.Id && u1.Name == u2.Name)));

            // Assert
            Assert.Equal(new[] { new DiffResultUpdatedItems<User>(source[1], target[1]) }, diff.Updated);
            Assert.Equal(new[] { new DiffResultUpdatedItems<User>(source[1], target[1]) }, diffEq.Updated);
        }

        [Fact]
        public void Diff_TargetCollectionWithElements_ElementsWereAddedRemovedUpdated()
        {
            // Arrange
            var source = new List<User>
            {
                new(1, "Ivan"),
                new(2, "Roma"),
                new(3, "Vlad"),
                new(4, "Denis"),
                new(5, "Nastya"),
                new(6, "Marina"),
                new(7, "Varvara"),
            };
            var target = new List<User>
            {
                new(1, "Ivan"),
                new(2, "Roman"),
                new(5, "Anastasya"),
                new(6, "Marina"),
                new(7, "Varvara"),
                new(0, "Tamara"),
                new(0, "Pavel"),
            };

            // Act
            var diff = CollectionUtils.Diff(source, target, (u1, u2) => u1.Id == u2.Id);
            var diffEq = CollectionUtils.Diff(source, target, new UserIdentityEqualityComparer());

            // Assert
            Assert.Equal(2, diff.Added.Count);
            Assert.Equal(5, diff.Updated.Count);
            Assert.Equal(2, diff.Removed.Count);
            Assert.Equal(2, diffEq.Added.Count);
            Assert.Equal(5, diffEq.Updated.Count);
            Assert.Equal(2, diffEq.Removed.Count);
        }

        [Fact]
        public void ApplyDiff_TargetCollectionWithElements_AfterApplyCollectionsAreEqual()
        {
            // Arrange
            var source = new List<User>
            {
                new(1, "Ivan"),
                new(2, "Roma"),
                new(3, "Vlad"),
                new(4, "Denis"),
                new(5, "Nastya"),
                new(6, "Marina"),
                new(7, "Varvara"),
            };
            var target = new List<User>
            {
                new(1, "Ivan"),
                new(2, "Roman"),
                new(5, "Anastasya"),
                new(6, "Marina"),
                new(7, "Varvara"),
                new(0, "Tamara"),
                new(0, "Pavel"),
            };

            // Act
            var diff = CollectionUtils.Diff(source, target, (u1, u2) => u1.Id == u2.Id);
            CollectionUtils.ApplyDiff(source, diff, (source, target) =>
            {
                source.Name = target.Name;
            });

            // Assert
            Assert.Equal(target.OrderBy(u => u.Id), source.OrderBy(u => u.Id), new UserEqualityComparer());
        }

        [Fact]
        public void ApplyDiffEqComparer_TargetCollectionWithElements_AfterApplyCollectionsAreEqual()
        {
            // Arrange
            var source = new List<User>
            {
                new(1, "Ivan"),
                new(2, "Roma"),
                new(3, "Vlad"),
                new(4, "Denis"),
                new(5, "Nastya"),
                new(6, "Marina"),
                new(7, "Varvara"),
            };
            var target = new List<User>
            {
                new(1, "Ivan"),
                new(2, "Roman"),
                new(5, "Anastasya"),
                new(6, "Marina"),
                new(7, "Varvara"),
                new(0, "Tamara"),
                new(0, "Pavel"),
            };

            // Act
            var diff = CollectionUtils.Diff(source, target, new UserIdentityEqualityComparer());
            CollectionUtils.ApplyDiff(source, diff, (source, target) =>
            {
                source.Name = target.Name;
            });

            // Assert
            Assert.Equal(target.OrderBy(u => u.Id), source.OrderBy(u => u.Id), new UserEqualityComparer());
        }

        [Fact]
        public void OrderParsingDelegates_QueryString_CollectionOfSortEntries()
        {
            // Arrange
            var query = "id:asc,name:desc;year salary;year:desc";

            // Act
            var sortingEntries = OrderParsingDelegates.ParseSeparated(query);

            // Assert
            var expected = new[]
            {
                ("id", ListSortDirection.Ascending),
                ("name", ListSortDirection.Descending),
                ("year salary", ListSortDirection.Ascending),
                ("year", ListSortDirection.Descending)
            };
            Assert.Equal(expected, sortingEntries);
        }

        [Fact]
        public void OrderMultiple_NotOrderedQueryableCollectionOfUsers_OrderedCollection()
        {
            // Arrange
            var source = new List<User>
            {
                new(1, "A"),
                new(4, "B"),
                new(4, "Z"),
                new(2, "D")
            };

            // Act
            var target = CollectionUtils.OrderMultiple(
                source.AsQueryable(),
                OrderParsingDelegates.ParseSeparated("id:asc;name:desc"),
                ("id", u => u.Id),
                ("name", u => u.Name)
            );

            // Assert
            var expected = new List<User>
            {
                new(1, "A"),
                new(2, "D"),
                new(4, "Z"),
                new(4, "B")
            };
            Assert.Equal(expected, target, new UserEqualityComparer());
        }

        [Fact]
        public void OrderMultiple_NotOrderedListOfUsers_OrderedCollection()
        {
            // Arrange
            var source = new List<User>
            {
                new(1, "A"),
                new(4, "B"),
                new(4, "Z"),
                new(2, "D")
            };

            // Act
            var target = CollectionUtils.OrderMultiple(
                source,
                OrderParsingDelegates.ParseSeparated("id:asc;name:desc"),
                ("id", u => u.Id),
                ("name", u => u.Name)
            );

            // Assert
            var expected = new List<User>
            {
                new(1, "A"),
                new(2, "D"),
                new(4, "Z"),
                new(4, "B")
            };
            Assert.Equal(expected, target, new UserEqualityComparer());
        }

        [Fact]
        public void OrderMultiple_EmptyOrderList_ReturnsSourceCollection()
        {
            // Arrange
            var source = new List<User>
            {
                new(2, "B"),
                new(1, "A")
            };

            // Act
            var target = CollectionUtils.OrderMultiple(
                source,
                OrderParsingDelegates.ParseSeparated(string.Empty),
                ("id", u => u.Id)
            );

            // Assert
            var expected = new List<User>
            {
                new(2, "B"),
                new(1, "A")
            };
            Assert.Equal(expected, target, new UserEqualityComparer());
        }

        [Fact]
        public void OrderMultiple_SelectorWithDoubleKey_OrderWithOrderThen()
        {
            // Arrange
            var source = new List<User>
            {
                new(1, "B"),
                new(1, "A"),
                new(2, "C"),
            };

            // Act
            var target = CollectionUtils.OrderMultiple(
                source,
                OrderParsingDelegates.ParseSeparated("key:asc"),
                ("key", u => u.Id),
                ("key", u => u.Name)
            );

            // Assert
            var expected = new List<User>
            {
                new(1, "A"),
                new(1, "B"),
                new(2, "C"),
            };
            Assert.Equal(expected, target, new UserEqualityComparer());
        }

        [Fact]
        public void OrderMultiple_OrderByNotExistingKey_InvalidOrderFieldException()
        {
            // Arrange
            var source = new List<User>
            {
                new(1, "B"),
            };

            // Assert
            Assert.Throws<InvalidOrderFieldException>(() =>
            {
                var target = CollectionUtils.OrderMultiple(
                    source,
                    OrderParsingDelegates.ParseSeparated("date:asc"),
                    ("id", u => u.Id),
                    ("name", u => u.Name)
                );
            });
        }

        [Fact]
        public void OrderMultiple_OrderWithCamelCaseKey_KeysMatchShouldBeCaseInsensitive()
        {
            // Arrange
            var source = new List<User>
            {
                new(1, "B"),
                new(2, "A")
            };

            // Act
            var target = CollectionUtils.OrderMultiple(
                source,
                OrderParsingDelegates.ParseSeparated("name:asc"),
                ("Name", u => u.Name)
            );

            // Assert
            var expected = new List<User>
            {
                new User(2, "A"),
                new User(1, "B")
            };
            Assert.Equal(expected, target, new UserEqualityComparer());
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
            foreach (var sublist in CollectionUtils.ChunkSelectRange(list.AsQueryable(), 45))
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
        public async System.Threading.Tasks.Task ChunkSelectAsync_EnumerableWithItems_ShouldIterateWholeList()
        {
            // Arrange
            static async IAsyncEnumerable<int> GetInts()
            {
                for (int i = 0; i < 250; i++)
                {
                    yield return i;
                }

                await System.Threading.Tasks.Task.CompletedTask;
            }
            int sum = 0;

            // Act
            await foreach (var subitems in CollectionUtils.ChunkSelectRangeAsync(GetInts(), 250))
            {
                await foreach (var item in subitems)
                {
                    sum += item;
                }
            }

            // Assert
            Assert.Equal(31125, sum);
        }

        [Fact]
        public async System.Threading.Tasks.Task ChunkSelectAsync_EnumerableWithItems_ShouldSkipItems()
        {
            // Arrange
            static async IAsyncEnumerable<int> GetInts()
            {
                for (int i = 0; i < 250; i++)
                {
                    yield return i;
                }

                await System.Threading.Tasks.Task.CompletedTask;
            }
            int iterations = 0;

            // Act
            await foreach (var subitems in CollectionUtils.ChunkSelectRangeAsync(GetInts(), 25, 25))
            {
                iterations++;
                await foreach (var item in subitems)
                {
                }
            }

            // Assert
            Assert.Equal(9, iterations);
        }

        [Fact]
        public void Pairwise_List_ShouldCreateCorrectly()
        {
            // Arrange
            var collection = new int[]
            {
                1,
                2,
                3,
                4,
            };

            // Act
            var pairwisedCollection = CollectionUtils.Pairwise(collection).ToList();

            // Assert
            var expected = new List<(int, int)>
            {
                (1, 2),
                (2, 3),
                (3, 4)
            };
            Assert.Equal(expected, pairwisedCollection);
        }

        [Fact]
        public void Pairwise_List_EmptyIfOneElement()
        {
            // Arrange
            var collection = new int[]
            {
                1,
            };

            // Act
            var pairwisedCollection = CollectionUtils.Pairwise(collection).ToList();

            // Assert
            Assert.Empty(pairwisedCollection);
        }
    }
}
