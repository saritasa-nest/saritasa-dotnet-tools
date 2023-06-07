// Copyright (c) 2015-2023, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Saritasa.Tools.Domain.Exceptions;

namespace Saritasa.Tools.EFCore
{
    /// <summary>
    /// <see cref="IQueryable{T}" /> extensions.
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Get entity instance by predicate of generates <see cref="NotFoundException" /> exception.
        /// </summary>
        /// <param name="entities">Entities collection.</param>
        /// <param name="predicate">Predicate to filter.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>Entity instance.</returns>
        /// <exception cref="NotFoundException">Is thrown if the entity not found.</exception>
        public static async Task<TEntity> GetAsync<TEntity>(
            this IQueryable<TEntity> entities,
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
                where TEntity : class
        {
            var entity = await entities.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
            if (entity == null)
            {
                throw new NotFoundException(string.Format(Properties.Strings.CannotFindEntity, typeof(TEntity).Name));
            }
            return entity;
        }
    }
}
