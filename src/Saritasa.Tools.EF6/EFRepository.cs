// Copyright (c) 2015-2019, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Domain;
using Saritasa.Tools.Domain.Exceptions;

namespace Saritasa.Tools.EF
{
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
        /// Entity set.
        /// </summary>
        public DbSet<TEntity> Set { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">Database context.</param>
        public EFRepository(TContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Context = context;
            Set = Context.Set<TEntity>();
        }

        #region IRepository

        /// <inheritdoc />
        public virtual TEntity Get(params object[] keyValues)
        {
            var entity = Set.Find(keyValues);
            if (entity == null)
            {
                throw new NotFoundException(Properties.Strings.ObjectNotFound);
            }
            return entity;
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> GetAll(Expression<Func<TEntity, object>>[] includes)
        {
            return Set.Include(includes).ToArray();
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> Find(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, object>>[] includes)
        {
            return Set.Where(predicate).Include(includes).ToArray();
        }

        /// <inheritdoc />
        public virtual void Add(TEntity entity)
        {
            Set.Add(entity);
        }

        /// <inheritdoc />
        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            Set.AddRange(entities);
        }

        /// <inheritdoc />
        public virtual void Remove(TEntity entity)
        {
            Set.Remove(entity);
        }

        /// <inheritdoc />
        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            Set.RemoveRange(entities);
        }

        #endregion

        #region IAsyncRepository

        /// <inheritdoc />
        public virtual async Task<TEntity> GetAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            var entity = await Set.FindAsync(cancellationToken, keyValues).ConfigureAwait(false);
            if (entity == null)
            {
                throw new NotFoundException(Properties.Strings.ObjectNotFound);
            }
            return entity;
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
            CancellationToken cancellationToken,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = Set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = Context.Set<TEntity>().Where(predicate);
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual Task AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            Set.Add(entity);
            return InternalHelpers.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken)
        {
            Set.Remove(entity);
            return InternalHelpers.CompletedTask;
        }

        #endregion
    }
}
