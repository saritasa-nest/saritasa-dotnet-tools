// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Saritasa.Tools.Common.Pagination
{
    /// <summary>
    /// Represents enumerable with additional property contains total number of objects within
    /// collection. It is subset of source collection with total count of base collection.
    /// The class does not evaluate source query.
    /// </summary>
    /// <seealso cref="IEnumerable{T}" />
    public class TotalCountEnumerable<T> : IEnumerable<T>, IMetadataEnumerable<TotalCountEnumerableMetadata>
    {
        /// <summary>
        /// Internal .ctor
        /// </summary>
        internal TotalCountEnumerable()
        {
        }

        /// <summary>
        /// Creates enumerable with total count.
        /// </summary>
        /// <param name="source">Enumerable.</param>
        /// <param name="totalCount">Total count of items of base collection.</param>
        internal TotalCountEnumerable(
            IEnumerable<T> source,
            int totalCount = -1)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.Source = source;
            this.TotalCount = totalCount > -1 ? totalCount : source.Count();
        }

        /// <summary>
        /// Creates enumerable with total count.
        /// </summary>
        /// <param name="source">Queryable enumerable</param>
        /// <param name="totalCount">Total count of items of base collection. If below zero query
        /// will be evaluated automatically.</param>
        internal TotalCountEnumerable(
            IQueryable<T> source,
            int totalCount = -1)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.Source = source;
            this.TotalCount = totalCount > -1 ? totalCount : source.Count();
        }

        /// <summary>
        /// Source enumerable.
        /// </summary>
        protected internal IEnumerable<T> Source { get; set; }

        /// <summary>
        /// Total number of items of base collection.
        /// </summary>
        public int TotalCount { get; protected internal set; }

        /// <summary>
        /// Get metadata object.
        /// </summary>
        /// <returns>Metadata.</returns>
        public TotalCountEnumerableMetadata GetMetadata()
        {
            return new TotalCountEnumerableMetadata
            {
                TotalCount = TotalCount
            };
        }

        #region Enumerator

        /// <summary>
        /// Returns enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Source.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
