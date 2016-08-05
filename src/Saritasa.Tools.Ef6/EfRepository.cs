// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Ef
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using Domain;

    /// <summary>
    /// Entity Framework repository implementation.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TContext">Database context type.</typeparam>
    public class EfRepository<TEntity, TContext> : IRepository<TEntity>
        where TEntity : class where TContext : DbContext
    {
        /// <summary>
        /// Database context.
        /// </summary>
        protected TContext Context { get; private set; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="context">Database context.</param>
        public EfRepository(TContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Context = context;
        }

        /// <inheritdoc />
        public void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        /// <inheritdoc />
        public void AddRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().AddRange(entities);
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Where(predicate);
        }

        /// <inheritdoc />
        public TEntity Get(object id)
        {
            return Context.Set<TEntity>().Find(id);
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> GetAll()
        {
            return Context.Set<TEntity>().ToList();
        }

        /// <inheritdoc />
        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        /// <inheritdoc />
        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
        }
    }
}
