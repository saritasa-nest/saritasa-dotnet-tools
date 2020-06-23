// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Represents list with additional property contains the total number of objects within
    /// collection.
    /// </summary>
    /// <seealso cref="IEnumerable{T}" />
#if NET40 || NETSTANDARD2_0 || NETSTANDARD2_1
    [Serializable]
#endif
    public class TotalCountList<T> : IEnumerable<T>
    {
        /// <summary>
        /// Source collection.
        /// </summary>
        protected internal ICollection<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// The total number of items in collection.
        /// </summary>
        public int TotalCount { get; protected internal set; }

        /// <summary>
        /// Gets the element at the specified index. It only works for list collections.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        public T this[int index] => ((IList<T>)Items)[index];

        /// <summary>
        /// Empty total count list.
        /// </summary>
        public static TotalCountList<T> Empty { get; } =
            new TotalCountList<T>(new List<T>(), totalCount: 0);

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        protected TotalCountList()
        {
        }

#if NET40 || NETSTANDARD2_0 || NETSTANDARD2_1
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
        /// <param name="totalCount">The total number of items of base collection.</param>
        public TotalCountList(ICollection<T> items, int totalCount)
        {
            if (totalCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalCount));
            }

            this.Items = items ?? throw new ArgumentNullException(nameof(items));
            this.TotalCount = totalCount;
        }

        #region IMetadataEnumerable<TotalCountEnumerableMetadata, T>

        /// <summary>
        /// Get the metadata object.
        /// </summary>
        /// <returns>Metadata.</returns>
        private TotalCountListMetadata GetMetadata()
        {
            return new TotalCountListMetadata
            {
                TotalCount = TotalCount
            };
        }

        /// <summary>
        /// Create items object with metadata.
        /// </summary>
        /// <returns>List metadata.</returns>
        public TotalCountListMetadataDto<T> ToMetadataObject()
            => new TotalCountListMetadataDto<T>(Items, GetMetadata());

        /// <summary>
        /// Convert items into another type.
        /// </summary>
        /// <param name="converter">Converter function.</param>
        /// <typeparam name="TNew">New type.</typeparam>
        /// <returns>New list.</returns>
        public TotalCountList<TNew> Convert<TNew>(Func<T, TNew> converter)
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
