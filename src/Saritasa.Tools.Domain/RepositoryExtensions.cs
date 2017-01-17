// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Domain
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Exceptions;
    using JetBrains.Annotations;

    /// <summary>
    /// <see cref="IRepository{TEntity}" /> extension methods.
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// A convenience method for looking up an object with the given PKs, creating one if necessary.
        /// Returns existing object from database on new one.
        /// </summary>
        /// <typeparam name="TEntity">Entity type of repository. Must have parameterless constructor.</typeparam>
        /// <param name="repository">Repository.</param>
        /// <param name="keyValues">Key values to lookup by.</param>
        /// <returns>Existing entity from repository or newly created one.</returns>
        [NotNull]
        public static TEntity GetOrAdd<TEntity>(
            [NotNull] IRepository<TEntity> repository,
            params object[] keyValues) where TEntity : class, new()
        {
            var entity = repository.Get(keyValues);
            if (entity == null)
            {
                entity = new TEntity();
                repository.Add(entity);
            }
            return entity;
        }

        /// <summary>
        /// A convenience method for looking up an object with the given delegate, creating one if necessary.
        /// Returns existing object from database on new one.
        /// </summary>
        /// <typeparam name="TEntity">Entity type of repository. Must have parameterless constructor.</typeparam>
        /// <param name="repository">Repository.</param>
        /// <param name="expression">Expression to lookup the entity.</param>
        /// <returns>Existing entity from repository or newly created one.</returns>
        [NotNull]
        public static TEntity GetOrAdd<TEntity>(
            [NotNull] IRepository<TEntity> repository,
            Expression<Func<TEntity, bool>> expression) where TEntity : class, new()
        {
            var entity = repository.Find(expression).Single();
            if (entity == null)
            {
                entity = new TEntity();
                repository.Add(entity);
            }
            return entity;
        }

        /// <summary>
        /// A convenience method for looking up an object with the given PKs. Throws
        /// <see cref="NotFoundException" /> if no records were found.
        /// </summary>
        /// <typeparam name="TEntity">Entity type of repository.</typeparam>
        /// <param name="repository">Repository.</param>
        /// <param name="keyValues">Key values to lookup by.</param>
        /// <returns>Existing entity from repository.</returns>
        [NotNull]
        public static TEntity GetOrThrow<TEntity>(
            [NotNull] IRepository<TEntity> repository,
            params object[] keyValues) where TEntity : class
        {
            var entity = repository.Get(keyValues);
            if (entity == null)
            {
                throw new NotFoundException($"Cannot find {typeof(TEntity).Name}");
            }
            return entity;
        }

        /// <summary>
        /// A convenience method for looking up an object with the given delegate. Throws
        /// <see cref="NotFoundException" /> if no records were found.
        /// </summary>
        /// <typeparam name="TEntity">Entity type of repository.</typeparam>
        /// <param name="repository">Repository.</param>
        /// <param name="expression">Expression to lookup the entity.</param>
        /// <returns>Existing entity from repository.</returns>
        [NotNull]
        public static TEntity GetOrThrow<TEntity>(
            [NotNull] IRepository<TEntity> repository,
            Expression<Func<TEntity, bool>> expression) where TEntity : class
        {
            var entity = repository.Find(expression).Single();
            if (entity == null)
            {
                throw new NotFoundException($"Cannot find {typeof(TEntity).Name}");
            }
            return entity;
        }
    }
}
