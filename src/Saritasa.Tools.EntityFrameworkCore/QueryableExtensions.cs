// Copyright (c) 2015-2025, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Saritasa.Tools.Domain.Exceptions;
using Saritasa.Tools.Domain.Localization;
using Strings = Saritasa.Tools.EntityFrameworkCore.Properties.Strings;

namespace Saritasa.Tools.EntityFrameworkCore;

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
            var errorMessage = new FormattedString<Strings>(
                Strings.CannotFindEntity.AsLocalizedString(),
                typeof(TEntity).Name);

            throw new NotFoundException(errorMessage);
        }
        return entity;
    }
}
