// Copyright (c) 2015-2023, Saritasa. All rights reserved.
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
    /// <see cref="TotalCountList{T}" /> and is intended to simplify instantiation and better API.
    /// </summary>
    public static class EFTotalCountListFactory
    {
        /// <summary>
        /// Creates a list with the total count from the queryable source.
        /// The calling will evaluate the query automatically.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="source">Queryable enumerable.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Total count list.</returns>
        public static async Task<TotalCountList<T>> FromSourceAsync<T>(
            IQueryable<T> source,
            CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new TotalCountList<T>(
                await source.ToListAsync(cancellationToken),
                await source.CountAsync(cancellationToken)
            );
        }
    }
}
