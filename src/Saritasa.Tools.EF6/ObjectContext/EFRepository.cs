// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using Saritasa.Tools.Domain;
using System.Data.Entity.Core.Objects.DataClasses;

namespace Saritasa.Tools.EF.ObjectContext
{
    /// <summary>
    /// Entity Framework repository implementation. Based on <see cref="System.Data.Entity.Core.Objects.ObjectContext" />.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TContext">Database context type.</typeparam>
    public class EFRepository<TEntity, TContext> : IRepository<TEntity>
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
            return Set.Where(predicate);
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> Find<TProperty>(Expression<Func<TEntity, bool>> predicate,
            IEnumerable<Expression<Func<TEntity, TProperty>>> includes)
        {
            return Set.Where(predicate).Include(includes);
        }

        /// <inheritdoc />
        public virtual TEntity Get(params object[] keyValues)
        {
            switch (keyValues.Length)
            {
                case 1:
                    return Set.SingleOrDefault(e => e.EntityKey.EntityKeyValues[0].Value == keyValues[0]);
                case 2:
                    return Set.SingleOrDefault(e => e.EntityKey.EntityKeyValues[0].Value == keyValues[0]
                        && e.EntityKey.EntityKeyValues[1].Value == keyValues[1]);
                case 3:
                    return Set.SingleOrDefault(e => e.EntityKey.EntityKeyValues[0].Value == keyValues[0]
                                                    && e.EntityKey.EntityKeyValues[1].Value == keyValues[1]
                                                    && e.EntityKey.EntityKeyValues[2].Value == keyValues[2]);
            }
            throw new InvalidKeyException(Properties.Strings.InvalidKeyCount);
        }

        /// <inheritdoc />
        public virtual TEntity Get<TProperty>(IEnumerable<Expression<Func<TEntity, TProperty>>> includes, params object[] keyValues)
        {
            var set = (ObjectQuery<TEntity>)Set;
            foreach (var include in includes)
            {
                set = set.Include(include.Name);
            }
            return Get(keyValues);
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> GetAll()
        {
            return Set.ToList();
        }

        /// <inheritdoc />
        public virtual IEnumerable<TEntity> GetAll<TProperty>(IEnumerable<Expression<Func<TEntity, TProperty>>> includes)
        {
            return Set.Include(includes).ToList();
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
    }
}
