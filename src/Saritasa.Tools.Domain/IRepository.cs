// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// Repository abstraction.
    /// </summary>
    /// <typeparam name="TEntity">The entity that repository wraps.</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        #region Sync methods

        /// <summary>
        /// Returns entity instance by id. In some implementations it is not the same as getting item by id
        /// with Single or First method. It may return cached item if it already exists in identity map.
        /// If entity is not found the <see cref="Saritasa.Tools.Domain.Exceptions.NotFoundException" /> will be
        /// generated.
        /// </summary>
        /// <param name="keyValues">Entity ids.</param>
        /// <exception cref="Saritasa.Tools.Domain.Exceptions.NotFoundException">Is generated when entity is not found.</exception>
        /// <returns>Entity instance.</returns>
        TEntity Get(params object[] keyValues);

        /// <summary>
        /// Get all entities of specified type.
        /// </summary>
        /// <param name="includes">Relations to include.</param>
        /// <returns>Enumerable of entities.</returns>
        IEnumerable<TEntity> GetAll(
            params Expression<Func<TEntity, object>>[] includes);

        /// <summary>
        /// Finds for range of entities based on predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate.</param>
        /// <param name="includes">Relations to include.</param>
        /// <returns>Enumerable of enitites.</returns>
        IEnumerable<TEntity> Find(
            Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includes);

        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        void Add(TEntity entity);

        /// <summary>
        /// Add range of entities.
        /// </summary>
        /// <param name="entities">Entities.</param>
        void AddRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Remove entity instance.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        void Remove(TEntity entity);

        /// <summary>
        /// Remove range of entities.
        /// </summary>
        /// <param name="entities">Entities.</param>
        void RemoveRange(IEnumerable<TEntity> entities);

        #endregion

        #region Async methods

        /// <summary>
        /// Returns entity instance by id. In some implementations it is not the same as getting item by id
        /// with Single or First method. It may return cached item if it already exists in identity map.
        /// If entity is not found the <see cref="Saritasa.Tools.Domain.Exceptions.NotFoundException" /> will be
        /// generated.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="keyValues">Entity identifications.</param>
        /// <exception cref="Saritasa.Tools.Domain.Exceptions.NotFoundException">Is generated when entity is not found.</exception>
        /// <returns>Task with result of entity instance.</returns>
        Task<TEntity> GetAsync(
            CancellationToken cancellationToken = default(CancellationToken),
            params object[] keyValues);

        /// <summary>
        /// Get all entities of specified type.
        /// </summary>
        /// <param name="includes">Relations to include.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Task with result of enumerable of entities.</returns>
        Task<IEnumerable<TEntity>> GetAllAsync(
            CancellationToken cancellationToken = default(CancellationToken),
            params Expression<Func<TEntity, object>>[] includes);

        /// <summary>
        /// Finds for range of entities based on predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate.</param>
        /// <param name="includes">Relations to include.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>Task with result of enumerable of enitites.</returns>
        Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default(CancellationToken),
            params Expression<Func<TEntity, object>>[] includes);

        /// <summary>
        /// Remove entity instance.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));

        #endregion
    }
}
