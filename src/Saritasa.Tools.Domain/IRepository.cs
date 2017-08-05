// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Saritasa.Tools.Domain
{
    /// <summary>
    /// Repository abstraction.
    /// <typeparam name="TEntity">The entity that repository wraps.</typeparam>
    /// </summary>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Returns entity instance by id. In some implementations it is not the same as getting item by id
        /// with Single or First method. It may return cached item if it already exists in identity map.
        /// If entity is not found the <see cref="Saritasa.Tools.Domain.Exceptions.NotFoundException" /> will be
        /// generated;
        /// </summary>
        /// <param name="keyValues">Entity ids.</param>
        /// <exception cref="Saritasa.Tools.Domain.Exceptions.NotFoundException">Is generated when entity is not found.</exception>
        /// <returns>Entity instance.</returns>
        TEntity Get(params object[] keyValues);

        /// <summary>
        /// Get all entities of specified type.
        /// </summary>
        /// <returns>Enumerable of entities.</returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        /// Get all entities of specified type.
        /// </summary>
        /// <param name="includes">Relations to include.</param>
        /// <returns>Enumerable of entities.</returns>
        IEnumerable<TEntity> GetAll(
            IEnumerable<Expression<Func<TEntity, object>>> includes);

        /// <summary>
        /// Find for range of entities based on predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate.</param>
        /// <returns>Enumerable of enitites.</returns>
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Finds for range of entities based on predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate.</param>
        /// <param name="includes">Relations to include</param>
        /// <returns>Enumerable of enitites.</returns>
        IEnumerable<TEntity> Find(
            Expression<Func<TEntity, bool>> predicate,
            IEnumerable<Expression<Func<TEntity, object>>> includes);

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
    }
}
