// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Core;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Domain;
using Saritasa.Tools.Domain.Exceptions;

namespace Saritasa.Tools.EF.ObjectContext
{
    /// <summary>
    /// Entity Framework repository implementation. Based on <see cref="System.Data.Entity.Core.Objects.ObjectContext" />.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TContext">Database context type.</typeparam>
    public class EFRepository<TEntity, TContext> : IRepository<TEntity>, IAsyncRepository<TEntity>
        where TEntity : EntityObject
        where TContext : System.Data.Entity.Core.Objects.ObjectContext
    {
        /// <summary>
        /// Database context.
        /// </summary>
        protected TContext Context { get; }

        /// <summary>
        /// Entity set.
        /// </summary>
        public ObjectSet<TEntity> Set { get; }

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
            Set = Context.CreateObjectSet<TEntity>();
        }

        #region IRepository

        /// <inheritdoc />
        public virtual void Add(TEntity entity)
        {
            Set.AddObject(entity);
        }

        /// <inheritdoc />
        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Set.AddObject(entity);
            }
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return Set.Where(predicate).ToArray();
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate,
            IEnumerable<Expression<Func<TEntity, object>>> includes)
        {
            return Set.Where(predicate).Include(includes).ToArray();
        }

        /// <inheritdoc />
        public virtual TEntity Get(params object[] keyValues)
        {
            // Get entity keys.
            var keyNames = Set.EntitySet.ElementType.KeyMembers.Select(k => k.Name).ToArray();
            if (keyNames.Length != keyValues.Length)
            {
                throw new InvalidKeyException(string.Format(Properties.Strings.InvalidKeyCount, keyValues.Length, keyNames.Length));
            }

            // Format key-value pairs.
            var keyValuePairs = new List<KeyValuePair<string, object>>(keyNames.Length);
            for (int i = 0; i < keyValues.Length; i++)
            {
                keyValuePairs.Add(new KeyValuePair<string, object>(keyNames[i], keyValues[i]));
            }

            // Select.
            var entityKey = new EntityKey(Context.DefaultContainerName + "." + Set.EntitySet.Name, keyValuePairs);
            try
            {
                return (TEntity)Context.GetObjectByKey(entityKey);
            }
            catch (ObjectNotFoundException ex)
            {
                throw new NotFoundException(Properties.Strings.ObjectNotFound, ex);
            }
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> GetAll()
        {
            return Set.ToArray();
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> GetAll(IEnumerable<Expression<Func<TEntity, object>>> includes)
        {
            return Set.Include(includes).ToArray();
        }

        /// <inheritdoc />
        public virtual void Remove(TEntity entity)
        {
            Set.DeleteObject(entity);
        }

        /// <inheritdoc />
        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Set.DeleteObject(entity);
            }
        }

        #endregion

        #region IAsyncRepository

        /// <inheritdoc />
        public virtual Task AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            Set.AddObject(entity);
            return InternalHelpers.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            foreach (var entity in entities)
            {
                Set.AddObject(entity);
            }
            return InternalHelpers.CompletedTask;
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken)
        {
            return await Set.Where(predicate).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> FindAsync(
            Expression<Func<TEntity, bool>> predicate,
            IEnumerable<Expression<Func<TEntity, object>>> includes,
            CancellationToken cancellationToken)
        {
            var query = Set.Where(predicate);
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual Task<TEntity> GetAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            return Task.FromResult(Get(keyValues));
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await Set.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
            IEnumerable<Expression<Func<TEntity, object>>> includes,
            CancellationToken cancellationToken)
        {
            var query = Set.AsQueryable();
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual Task RemoveAsync(TEntity entity, CancellationToken cancellationToken)
        {
            Set.DeleteObject(entity);
            return InternalHelpers.CompletedTask;
        }

        /// <inheritdoc />
        public virtual Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            foreach (var entity in entities)
            {
                Set.DeleteObject(entity);
            }
            return InternalHelpers.CompletedTask;
        }

        #endregion
    }
}
