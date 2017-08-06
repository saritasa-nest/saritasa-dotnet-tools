// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// Async repository abstraction. Target implementation may not support all possible async
    /// operations or support <see cref="CancellationToken" /> monitor.
    /// </summary>
    /// <typeparam name="TEntity">The entity that repository wraps.</typeparam>
    public interface IAsyncRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Returns entity instance by id. In some implementations it is not the same as getting item by id
        /// with Single or First method. It may return cached item if it already exists in identity map.
        /// If entity is not found the <see cref="Saritasa.Tools.Domain.Exceptions.NotFoundException" /> will be
        /// generated;
        /// </summary>
        /// <param name="keyValues">Entity ids.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <exception cref="Saritasa.Tools.Domain.Exceptions.NotFoundException">Is generated when entity is not found.</exception>
        /// <returns>Entity instance.</returns>
        Task<TEntity> GetAsync(object[] keyValues, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get all entities of specified type.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Enumerable of entities.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get all entities of specified type.
        /// </summary>
        /// <param name="includes">Relations to include.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Enumerable of entities.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(
            IEnumerable<Expression<Func<TEntity, object>>> includes,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Find for range of entities based on predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Enumerable of enitites.</returns>
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Finds for range of entities based on predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate.</param>
        /// <param name="includes">Relations to include</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Enumerable of enitites.</returns>
        Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            IEnumerable<Expression<Func<TEntity, object>>> includes,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Add range of entities.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Remove entity instance.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Remove range of entities.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default(CancellationToken));
    }
}
