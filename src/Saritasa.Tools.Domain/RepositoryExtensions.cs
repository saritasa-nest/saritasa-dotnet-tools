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
    /// <see cref="IRepository{TEntity}" /> extension methods.
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Get all entities of specified type.
        /// </summary>
        /// <param name="repository">Repository instance.</param>
        /// <param name="includes">Relations to include.</param>
        /// <returns>Task with result of enumerable of entities.</returns>
        public static Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(
            this IRepository<TEntity> repository,
            params Expression<Func<TEntity, object>>[] includes) where TEntity : class
        {
            return repository.GetAllAsync(CancellationToken.None, includes);
        }

        /// <summary>
        /// Finds for range of entities based on predicate.
        /// </summary>
        /// <param name="repository">Repository instance.</param>
        /// <param name="predicate">Filter predicate.</param>
        /// <param name="includes">Includes.</param>
        /// <returns>Enumerable of enitites.</returns>
        public static Task<IEnumerable<TEntity>> FindAsync<TEntity>(
            this IRepository<TEntity> repository,
            Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includes) where TEntity : class
        {
            return repository.FindAsync(predicate, CancellationToken.None, includes);
        }

        /// <summary>
        /// Returns entity instance by id. In some implementations it is not the same as getting item by id
        /// with Single or First method. It may return cached item if it already exists in identity map.
        /// If entity is not found the <see cref="Saritasa.Tools.Domain.Exceptions.NotFoundException" /> will be
        /// generated.
        /// </summary>
        /// <param name="repository">Repository instance.</param>
        /// <param name="keyValues">Entity ids.</param>
        /// <exception cref="Saritasa.Tools.Domain.Exceptions.NotFoundException">Is generated when entity is not found.</exception>
        /// <returns>Task with result of entity instance.</returns>
        public static Task<TEntity> GetAsync<TEntity>(
            this IRepository<TEntity> repository,
            params object[] keyValues) where TEntity : class
        {
            return repository.GetAsync(CancellationToken.None, keyValues);
        }
    }
}
