// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Ef
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Custom extensions for IQueryable
    /// </summary>
    public static class QueryableExtensions
    {
        /// <summary>
        /// Apply includes to query
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="this"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> Include<TEntity>(this IQueryable<TEntity> @this, IEnumerable<Expression<Func<TEntity, object>>> includes)
            where TEntity : class
        {
            var query = @this;
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query;
        }
    }
}
