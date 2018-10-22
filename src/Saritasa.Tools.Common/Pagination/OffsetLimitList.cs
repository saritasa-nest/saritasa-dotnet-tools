// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Enumerable with limit and offset feature.
    /// </summary>
    public class OffsetLimitList<T> : TotalCountList<T>,
        IMetadataEnumerable<T, OffsetLimitListMetadata>
    {
        /// <summary>
        /// Number of items to skip.
        /// </summary>
        public int Offset { get; protected internal set; }

        /// <summary>
        /// Maximum number of items to take.
        /// </summary>
        public int Limit { get; protected internal set; }

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        protected OffsetLimitList()
        {
        }

        /// <summary>
        /// Creates new enumerable with limit and offset properties.
        /// </summary>
        /// <param name="items">Collection items.</param>
        /// <param name="offset">Number of items to skip.</param>
        /// <param name="limit">Maximum number of items to take.</param>
        /// <param name="totalCount">Total number of items in collection.</param>
        public OffsetLimitList(
            ICollection<T> items,
            int offset,
            int limit,
            int totalCount) : base(items, totalCount)
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
        }

        #region IMetadataEnumerable<OffsetLimitEnumerableMetadata, T>

        /// <summary>
        /// Get offset limit metadata object.
        /// </summary>
        /// <returns>Offset limit metadata.</returns>
        private OffsetLimitListMetadata GetMetadata()
        {
            return new OffsetLimitListMetadata
            {
                Limit = Limit,
                Offset = Offset,
                TotalCount = TotalCount
            };
        }

        /// <inheritdoc />
        public new MetadataDto<T, OffsetLimitListMetadata> ToMetadataObject()
            => new MetadataDto<T, OffsetLimitListMetadata>(Items, GetMetadata());

        /// <inheritdoc />
        public new IMetadataEnumerable<TNew, OffsetLimitListMetadata> Convert<TNew>(Func<T, TNew> converter)
            => new OffsetLimitList<TNew>
            {
                Items = Items.Select(converter).ToList(),
                Limit = Limit,
                Offset = Offset,
                TotalCount = TotalCount
            };

        #endregion
    }
}
