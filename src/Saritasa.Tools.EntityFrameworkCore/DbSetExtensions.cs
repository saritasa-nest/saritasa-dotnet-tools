// Copyright (c) 2015-2024, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Saritasa.Tools.Domain.Exceptions;

namespace Saritasa.Tools.EntityFrameworkCore;

/// <summary>
/// <see cref="DbSet{TEntity}" /> extensions.
/// </summary>
public static class DbSetExtensions
{
    /// <summary>
    /// Get the entity by key values. Throws <see cref="NotFoundException" /> if the entity not found.
    /// </summary>
    /// <param name="entities">DbSet instance.</param>
    /// <param name="keyValues">Keys.</param>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <returns>Entity instance.</returns>
    /// <exception cref="NotFoundException">Is thrown if the entity not found.</exception>
    public static async Task<TEntity> GetAsync<TEntity>(this DbSet<TEntity> entities, params object[] keyValues)
        where TEntity : class
    {
        var entity = await entities.FindAsync(keyValues).ConfigureAwait(false);
        if (entity == null)
        {
            var ids = string.Join(", ", keyValues.Select(k => k.ToString()));
            throw new NotFoundException(string.Format(Properties.Strings.CannotFindEntityWithIdentifier, typeof(TEntity).Name, ids));
        }
        return entity;
    }

    /// <summary>
    /// Get the entity by key values. Throws <see cref="NotFoundException" /> if the entity not found.
    /// </summary>
    /// <param name="entities">DbSet instance.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <param name="keyValues">Keys.</param>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <returns>Entity instance.</returns>
    /// <exception cref="NotFoundException">Is thrown if the entity not found.</exception>
    public static async Task<TEntity> GetAsync<TEntity>(this DbSet<TEntity> entities, CancellationToken cancellationToken, params object[] keyValues)
        where TEntity : class
    {
        var entity = await entities.FindAsync(keyValues, cancellationToken).ConfigureAwait(false);
        if (entity == null)
        {
            var ids = string.Join(", ", keyValues.Select(k => k.ToString()));
            throw new NotFoundException(string.Format(Properties.Strings.CannotFindEntityWithIdentifier, typeof(TEntity).Name, ids));
        }
        return entity;
    }
}
