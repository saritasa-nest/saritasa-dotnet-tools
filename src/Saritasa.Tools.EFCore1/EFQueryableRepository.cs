// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Saritasa.Tools.Domain;

namespace Saritasa.Tools.EFCore
{
    /// <summary>
    /// Entity Framework repository implementation that supports IQueryable.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <typeparam name="TContext">Database context type.</typeparam>
    public class EFQueryableRepository<TEntity, TContext> : EFRepository<TEntity, TContext>, IQueryableRepository<TEntity>,
        IAsyncEnumerableAccessor<TEntity>
        where TEntity : class where TContext : DbContext
    {
        readonly DbSet<TEntity> set;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="context">Database context.</param>
        public EFQueryableRepository(TContext context) : base(context)
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

        /// <inheritdoc />
        public IAsyncEnumerable<TEntity> AsyncEnumerable => ((IAsyncEnumerableAccessor<TEntity>)set).AsyncEnumerable;
    }
}
