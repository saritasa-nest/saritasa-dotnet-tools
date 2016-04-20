//
// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.
//

namespace Saritasa.Tools.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Repository abstraction.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Get all entities of specified type.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <returns>Enumerable of entities.</returns>
        IEnumerable<TEntity> GetAll<TEntity>() where TEntity : class;

        /// <summary>
        /// Get entity instance by id.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <param name="id">Entity id.</param>
        /// <returns>Entity instance.</returns>
        TEntity Get<TEntity>(object id) where TEntity : class;

        /// <summary>
        /// Add entity.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <param name="entity">Entity instance.</param>
        void Add<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Remove entity instance.
        /// </summary>
        /// <typeparam name="TEntity">Entity type.</typeparam>
        /// <param name="entity">Entity instance.</param>
        void Remove<TEntity>(TEntity entity) where TEntity : class;
    }
}
