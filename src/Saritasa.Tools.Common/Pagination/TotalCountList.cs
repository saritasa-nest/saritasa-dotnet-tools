// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Represents list with additional property contains total number of objects within
    /// collection.
    /// </summary>
    /// <seealso cref="IEnumerable{T}" />
#if NET40 || NET452 || NET461 || NETSTANDARD2_0
    [Serializable]
#endif
    public class TotalCountList<T> :
        IMetadataEnumerable<T, TotalCountListMetadata>
    {
        /// <summary>
        /// Source collection.
        /// </summary>
        protected internal ICollection<T> Items { get; set; }

        /// <summary>
        /// Total number of items in collection.
        /// </summary>
        public int TotalCount { get; protected internal set; }

        /// <summary>
        /// Gets the element at the specified index. Only works for list collections.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        public T this[int index] => ((IList<T>)Items)[index];

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        protected TotalCountList()
        {
        }

#if NET40 || NET452 || NET461 || NETSTANDARD2_0
        /// <summary>
        /// Constructor for deserialization.
        /// </summary>
        /// <param name="info">Stores all the data needed to serialize or deserialize an object.</param>
        /// <param name="context">Describes the source and destination of a given serialized stream,
        /// and provides an additional caller-defined context.</param>
        protected TotalCountList(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
        }
#endif

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="items">Collection items.</param>
        /// <param name="totalCount">Total number of items of base collection.</param>
        public TotalCountList(ICollection<T> items, int totalCount)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (totalCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalCount));
            }

            this.Items = items;
            this.TotalCount = totalCount;
        }

        #region IMetadataEnumerable<TotalCountEnumerableMetadata, T>

        /// <summary>
        /// Get metadata object.
        /// </summary>
        /// <returns>Metadata.</returns>
        private TotalCountListMetadata GetMetadata()
        {
            return new TotalCountListMetadata
            {
                TotalCount = TotalCount
            };
        }

        /// <inheritdoc />
        public MetadataDto<T, TotalCountListMetadata> ToMetadataObject()
            => new MetadataDto<T, TotalCountListMetadata>(Items, GetMetadata());

        /// <inheritdoc />
        public IMetadataEnumerable<TNew, TotalCountListMetadata> Convert<TNew>(Func<T, TNew> converter)
            => new TotalCountList<TNew>
            {
                Items = Items.Select(converter).ToList(),
                TotalCount = TotalCount
            };

        #endregion

        #region Enumerator

        /// <summary>
        /// Returns enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
