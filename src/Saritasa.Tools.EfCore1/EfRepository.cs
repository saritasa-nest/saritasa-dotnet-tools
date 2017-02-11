// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.EFCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.EntityFrameworkCore;
    using Domain;

    /// <summary>
    /// Entity Framework repository implementation.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TContext">Database context type.</typeparam>
    public class EFRepository<TEntity, TContext> : IRepository<TEntity>
        where TEntity : class where TContext : DbContext
    {
        /// <summary>
        /// Database context.
        /// </summary>
        protected TContext Context { get; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="context">Database context.</param>
        public EFRepository(TContext context)
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
        public IEnumerable<TEntity> Find<TProperty>(Expression<Func<TEntity, bool>> predicate,
            IEnumerable<Expression<Func<TEntity, TProperty>>> includes)
        {
            var query = Context.Set<TEntity>().Where(predicate);
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Got from http://stackoverflow.com/questions/29030472/dbset-doesnt-have-a-find-method-in-ef7
        /// </remarks>
        public virtual TEntity Get(params object[] keyValues)
        {
            return Context.Set<TEntity>().Find(keyValues);
        }

        /// <inheritdoc />
        public virtual TEntity Get<TProperty>(IEnumerable<Expression<Func<TEntity, TProperty>>> includes, params object[] keyValues)
        {
            return Get(keyValues);
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> GetAll()
        {
            return Context.Set<TEntity>().AsQueryable();
        }

        /// <inheritdoc />
        public IEnumerable<TEntity> GetAll<TProperty>(IEnumerable<Expression<Func<TEntity, TProperty>>> includes)
        {
            var query = Context.Set<TEntity>().AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return query;
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
