// Copyright (c) 2015-2019, Saritasa. All rights reserved.
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

        private sealed class UsersComparer : IComparer<User>
        {
            public int Compare(User x, User y) =>
                y != null && (x != null && (x.Id == y.Id && x.Name == y.Name)) ? 0 : -1;
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
            var actions = CollectionUtils.Diff(source, target);

            // Assert
            Assert.Equal(new[] { 4 }, actions.Added);
        }

        [Fact]
        public void Diff_TargetCollectionWithoutOldElements_OldElementsAreRemoved()
        {
            // Arrange
            var source = new[] { 1, 2, 3 };
            var target = new[] { 1, 4 };

            // Act
            var actions = CollectionUtils.Diff(source, target);

            // Assert
            Assert.Equal(new[] { 2, 3 }, actions.Removed);
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

            // Assert
            Assert.Equal(new[] { new DiffResultUpdatedItems<User>(source[1], target[1]) }, diff.Updated);
        }

        [Fact]
        public void Diff_TargetCollectionWithUpdatedElementsAndComparer_NotUpdatedElementsSkipped()
        {
            // Arrange
            var source = new[]
            {
                new User(1, "Doug"),
                new User(2, "Pasha")
            };
            var target = new[]
            {
                new User(1, "Doug"),
                new User(2, "Pavel")
            };

            // Act
            var diff = CollectionUtils.Diff(source, target, (u1, u2) => u1.Id == u2.Id, new UsersComparer());

            // Assert
            Assert.Equal(new[] { new DiffResultUpdatedItems<User>(source[1], target[1]) }, diff.Updated);
        }

        [Fact]
        public void Diff_TargetCollectionWithElements_ElementsWereAddedRemovedUpdated()
        {
            // Arrange
            var source = new List<User>
            {
                new User(1, "Ivan"),
                new User(2, "Roma"),
                new User(3, "Vlad"),
                new User(4, "Denis"),
                new User(5, "Nastya"),
                new User(6, "Marina"),
                new User(7, "Varvara"),
            };
            var target = new List<User>
            {
                new User(1, "Ivan"),
                new User(2, "Roman"),
                new User(5, "Anastasya"),
                new User(6, "Marina"),
                new User(7, "Varvara"),
                new User(0, "Tamara"),
                new User(0, "Pavel"),
            };

            // Act
            var diff = CollectionUtils.Diff(source, target, (u1, u2) => u1.Id == u2.Id);

            // Assert
            Assert.Equal(2, diff.Added.Count);
            Assert.Equal(5, diff.Updated.Count);
            Assert.Equal(2, diff.Removed.Count);
        }

        [Fact]
        public void ApplyDiff_TargetCollectionWithElements_AfterApplyCollectionsAreEqual()
        {
            // Arrange
            var initialCollection = new List<User>
            {
                new User(1, "Ivan"),
                new User(2, "Roma"),
                new User(3, "Vlad"),
                new User(4, "Denis"),
                new User(5, "Nastya"),
                new User(6, "Marina"),
                new User(7, "Varvara"),
            };
            var newCollection = new List<User>
            {
                new User(1, "Ivan"),
                new User(2, "Roman"),
                new User(5, "Anastasya"),
                new User(6, "Marina"),
                new User(7, "Varvara"),
                new User(0, "Tamara"),
                new User(0, "Pavel"),
            };

            // Act
            var diff = CollectionUtils.Diff(initialCollection, newCollection, (u1, u2) => u1.Id == u2.Id);
            CollectionUtils.ApplyDiff(initialCollection, diff, (source, target) =>
            {
                source.Name = target.Name;
            });

            // Assert
            Assert.Equal(newCollection.OrderBy(u => u.Id), initialCollection.OrderBy(u => u.Id), new UserEqualityComparer());
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
                new User(1, "A"),
                new User(4, "B"),
                new User(4, "Z"),
                new User(2, "D")
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
                new User(1, "A"),
                new User(2, "D"),
                new User(4, "Z"),
                new User(4, "B")
            };
            Assert.Equal(expected, target, new UserEqualityComparer());
        }

        [Fact]
        public void OrderMultiple_NotOrderedListOfUsers_OrderedCollection()
        {
            // Arrange
            var source = new List<User>
            {
                new User(1, "A"),
                new User(4, "B"),
                new User(4, "Z"),
                new User(2, "D")
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
                new User(1, "A"),
                new User(2, "D"),
                new User(4, "Z"),
                new User(4, "B")
            };
            Assert.Equal(expected, target, new UserEqualityComparer());
        }

        [Fact]
        public void OrderMultiple_EmptyOrderList_ReturnsSourceCollection()
        {
            // Arrange
            var source = new List<User>
            {
                new User(2, "B"),
                new User(1, "A")
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
                new User(2, "B"),
                new User(1, "A")
            };
            Assert.Equal(expected, target, new UserEqualityComparer());
        }

        [Fact]
        public void OrderMultiple_SelectorWithDoubleKey_OrderWithOrderThen()
        {
            // Arrange
            var source = new List<User>
            {
                new User(1, "B"),
                new User(1, "A"),
                new User(2, "C"),
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
                new User(1, "A"),
                new User(1, "B"),
                new User(2, "C"),
            };
            Assert.Equal(expected, target, new UserEqualityComparer());
        }

        [Fact]
        public void OrderMultiple_OrderByNotExistingKey_InvalidOrderFieldException()
        {
            // Arrange
            var source = new List<User>
            {
                new User(1, "B"),
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
            foreach (var item in CollectionUtils.ChunkSelect(list.AsQueryable(), 45))
            {
                sum += item;
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
    }
}
