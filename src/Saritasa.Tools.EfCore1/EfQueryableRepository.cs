// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.EfCore
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.EntityFrameworkCore;
    using Domain;

    /// <summary>
    /// Entity Framework repository implementation that supports IQueryable.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TContext">Database context type.</typeparam>
    public class EfQueryableRepository<TEntity, TContext> : EfRepository<TEntity, TContext>, IQueryableRepository<TEntity>
        where TEntity : class where TContext : DbContext
    {
        readonly DbSet<TEntity> set;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="context">Database context.</param>
        public EfQueryableRepository(TContext context) : base(context)
        {
            set = Context.Set<TEntity>();
        }

        /// <inheritdoc />
        public Type ElementType => typeof(TEntity);

        /// <inheritdoc />
        public Expression Expression => ((IQueryable<TEntity>)set).Expression;

        /// <inheritdoc />
        public IQueryProvider Provider => ((IQueryable<TEntity>)set).Provider;

        /// <inheritdoc />
        public IEnumerator<TEntity> GetEnumerator() => ((IQueryable<TEntity>)set).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IQueryable<TEntity>)set).GetEnumerator();
    }
}
