// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Enumerable with limit and offset feature. Additionally forces evaluation with Take and Skip methods.
    /// </summary>
    public class OffsetLimitEnumerable<T> : TotalCountEnumerable<T>,
        IMetadataEnumerable<OffsetLimitEnumerableMetadata, T>
    {
        /// <summary>
        /// The zero based offset from the first row.
        /// </summary>
        public int Offset { get; protected internal set; }

        /// <summary>
        /// How many rows to retrieve.
        /// </summary>
        public int Limit { get; protected internal set; }

        /// <summary>
        /// Internal .ctor
        /// </summary>
        internal OffsetLimitEnumerable()
        {
        }

        /// <summary>
        /// Creates new enumerable with limit and offset properties. The calling will
        /// evaluate query automatically.
        /// </summary>
        /// <param name="baseSource">Based enumerable source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="limit">Limit.</param>
        /// <param name="totalCount">Total number of items. If below zero will be evaluated automatically.</param>
        internal OffsetLimitEnumerable(
            IEnumerable<T> baseSource,
            int offset = OffsetLimitEnumerable.DefaultOffset,
            int limit = OffsetLimitEnumerable.DefaultLimitSize,
            int totalCount = -1) : base(baseSource, totalCount)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (limit < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(limit));
            }

            this.Offset = offset;
            this.Limit = limit;
            this.Source = baseSource.Skip(Offset).Take(Limit);
        }

        /// <summary>
        /// Creates new enumerable with limit and offset properties. The calling will
        /// evaluate query automatically.
        /// </summary>
        /// <param name="baseSource">Based queryable enumerable source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="limit">Limit.</param>
        /// <param name="totalCount">Total number of items. If below zero will be evaluated automatically.</param>
        internal OffsetLimitEnumerable(
            IQueryable<T> baseSource,
            int offset = OffsetLimitEnumerable.DefaultOffset,
            int limit = OffsetLimitEnumerable.DefaultLimitSize,
            int totalCount = -1)
        {
            if (baseSource == null)
            {
                throw new ArgumentNullException(nameof(baseSource));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (limit < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(limit));
            }

            this.Offset = offset;
            this.Limit = limit;
            this.Source = baseSource.Skip(Offset).Take(Limit).ToList();
            this.TotalCount = totalCount > -1 ? totalCount : baseSource.Count();
        }

        #region IMetadataEnumerable<OffsetLimitEnumerableMetadata, T>

        /// <summary>
        /// Get offset limit metadata object.
        /// </summary>
        /// <returns>Offset limit metadata.</returns>
        public new OffsetLimitEnumerableMetadata GetMetadata()
        {
            return new OffsetLimitEnumerableMetadata
            {
                Limit = Limit,
                Offset = Offset,
                TotalCount = TotalCount
            };
        }

        /// <inheritdoc />
        public new MetadataDto<T> ToMetadataObject()
        {
            return new MetadataDto<T>(this, this.GetMetadata());
        }

        /// <inheritdoc />
        public new IMetadataEnumerable<OffsetLimitEnumerableMetadata, TNew> CastMetadataEnumerable<TNew>(Func<T, TNew> converter)
        {
            return new OffsetLimitEnumerable<TNew>
            {
                Source = this.Select(converter),
                TotalCount = this.TotalCount,
                Limit = this.Limit,
                Offset = this.Offset
            };
        }

        #endregion
    }
}
