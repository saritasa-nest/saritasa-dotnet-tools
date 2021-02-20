// Copyright (c) 2015-2021, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Benchmarks
{
    /// <summary>
    /// Benchmarks for <see cref="CollectionUtils" /> Diff method.
    /// </summary>
    [MemoryDiagnoser]
    public class CollectionUtilsDiffBenchmark
    {
        /// <summary>
        /// Test class, for example user.
        /// </summary>
        private sealed class User : ICloneable
        {
            /// <summary>
            /// Identifier.
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// User name.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public User()
            {
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="id">Identifier.</param>
            public User(Guid id)
            {
                Id = id;
                Name = id.ToString("N");
            }

            private bool Equals(User other) => Id.Equals(other.Id) && Name == other.Name;

            /// <inheritdoc />
            public override bool Equals(object obj) => ReferenceEquals(this, obj) || obj is User other && Equals(other);

            /// <inheritdoc />
            public override int GetHashCode() => HashCode.Combine(Id, Name);

            /// <inheritdoc />
            public object Clone()
            {
                return new User
                {
                    Id = Id,
                    Name = Name
                };
            }
        }

        /// <summary>
        /// Identity comparer, it is used only to compare Id property.
        /// </summary>
        private class UserIdentityComparer : IEqualityComparer<User>
        {
            /// <inheritdoc />
            public bool Equals(User x, User y) => x.Id.Equals(y.Id);

            /// <inheritdoc />
            public int GetHashCode(User obj) => obj.Id.GetHashCode();
        }

        /// <summary>
        /// Full properties user comparer.
        /// </summary>
        private class UserComparer : IEqualityComparer<User>
        {
            /// <inheritdoc />
            public bool Equals(User x, User y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }
                if (ReferenceEquals(x, null))
                {
                    return false;
                }
                if (ReferenceEquals(y, null))
                {
                    return false;
                }
                if (x.GetType() != y.GetType())
                {
                    return false;
                }
                return x.Id.Equals(y.Id) && x.Name == y.Name;
            }

            /// <inheritdoc />
            public int GetHashCode(User obj)
            {
                return HashCode.Combine(obj.Id, obj.Name);
            }
        }

        private readonly IList<User> usersSource = new List<User>();

        private readonly IList<User> usersTarget = new List<User>();

        private readonly IEqualityComparer<User> usersIdentityComparerInstance = new UserIdentityComparer();

        private readonly IEqualityComparer<User> usersComparerInstance = new UserComparer();

        /// <summary>
        /// Constructor.
        /// </summary>
        public CollectionUtilsDiffBenchmark()
        {
            // New items.
            for (int i = 0; i < 30_000; i++)
            {
                usersTarget.Add(new User(Guid.NewGuid()));
            }

            // Updated items.
            for (int i = 0; i < 10_000; i++)
            {
                var user = (User)usersTarget[i].Clone();
                user.Name += "-updated";
                usersSource.Add(user);
            }

            // Copy items.
            for (int i = 10_000; i < 20_000; i++)
            {
                var user = (User)usersTarget[i].Clone();
                usersSource.Add(user);
            }

            // Removed items.
            for (int i = 0; i < 10_000; i++)
            {
                usersSource.Add(new User(Guid.NewGuid()));
            }
        }

        [Benchmark]
        public void SaritasaToolsDiff()
        {
            var diff = CollectionUtils.Diff(
                usersSource,
                usersTarget,
                identityComparer: (user1, user2) => user1.Id == user2.Id,
                dataComparer: (user1, user2) => user1.Name == user2.Name);
        }

        [Benchmark]
        public void SaritasaToolsDiffWithEqualityComparer()
        {
            var diff = CollectionUtils.Diff(
                usersSource,
                usersTarget,
                identityEqualityComparer: usersIdentityComparerInstance,
                dataComparer: (user1, user2) => user1.Name == user2.Name);
        }

        [Benchmark]
        public void ExperimentalDiffWithEqualityComparer()
        {
            var diff = DifferenceService.GetDiffResult(
                usersSource,
                usersTarget,
                usersIdentityComparerInstance);
        }
    }
}
