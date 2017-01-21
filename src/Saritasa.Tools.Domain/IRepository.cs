﻿// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using JetBrains.Annotations;

    /// <summary>
    /// Repository abstraction.
    /// <typeparam name="TEntity">The entity repository wraps.</typeparam>
    /// </summary>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Get entity instance by id.
        /// </summary>
        /// <param name="keyValues">Entity ids.</param>
        /// <returns>Entity instance.</returns>
        TEntity Get(params object[] keyValues);

        /// <summary>
        /// Get entity instance by id.
        /// </summary>
        /// <param name="includes">Relations to include.</param>
        /// <param name="keyValues">Entity ids.</param>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <returns>Entity instance.</returns>
        TEntity Get<TProperty>(
            [NotNull] IEnumerable<Expression<Func<TEntity, TProperty>>> includes,
            params object[] keyValues);

        /// <summary>
        /// Get all entities of specified type.
        /// </summary>
        /// <returns>Enumerable of entities.</returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        /// Get all entities of specified type.
        /// </summary>
        /// <param name="includes">Relations to include.</param>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <returns>Enumerable of entities.</returns>
        IEnumerable<TEntity> GetAll<TProperty>(
            [NotNull] IEnumerable<Expression<Func<TEntity, TProperty>>> includes);

        /// <summary>
        /// Find for range of entities based on predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate.</param>
        /// <returns>Enumerable of enitites.</returns>
        IEnumerable<TEntity> Find([NotNull] Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Finds for range of entities based on predicate.
        /// </summary>
        /// <param name="predicate">Filter predicate.</param>
        /// <param name="includes">Relations to include</param>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <returns>Enumerable of enitites.</returns>
        IEnumerable<TEntity> Find<TProperty>(
            [NotNull] Expression<Func<TEntity, bool>> predicate,
            [NotNull] IEnumerable<Expression<Func<TEntity, TProperty>>> includes);

        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        void Add([NotNull] TEntity entity);

        /// <summary>
        /// Add range of entities.
        /// </summary>
        /// <param name="entities">Entities.</param>
        void AddRange([NotNull] IEnumerable<TEntity> entities);

        /// <summary>
        /// Remove entity instance.
        /// </summary>
        /// <param name="entity">Entity instance.</param>
        void Remove([NotNull] TEntity entity);

        /// <summary>
        /// Remove range of entities.
        /// </summary>
        /// <param name="entities">Entities.</param>
        void RemoveRange([NotNull] IEnumerable<TEntity> entities);
    }
}