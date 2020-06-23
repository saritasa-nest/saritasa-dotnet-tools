// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Saritasa.Tools.Common.Pagination;

namespace Saritasa.Tools.EFCore.Pagination
{
    /// <summary>
    /// The class contains Entity Framework related static methods for
    /// <see cref="OffsetLimitList{T}" /> and is intended to simplify instantiation and better API.
    /// </summary>
    public static class EFOffsetLimitListFactory
    {
        /// <summary>
        /// Creates a new list with limit and offset properties from the queryable source.
        /// The calling will evaluate the query automatically.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Queryable source.</param>
        /// <param name="offset">The number of items to skip.</param>
        /// <param name="limit">The maximum number of items to take.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>List with offset and limit.</returns>
        public static async Task<OffsetLimitList<T>> FromSourceAsync<T>(
            IQueryable<T> source,
            int offset,
            int limit,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new OffsetLimitList<T>(
                await source.Skip(offset).Take(limit).ToListAsync(cancellationToken),
                offset,
                limit,
                await source.CountAsync(cancellationToken));
        }
    }
}
