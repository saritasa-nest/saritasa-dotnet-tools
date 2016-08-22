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
    public static class IQueryableExtensions
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
            IQueryable<TEntity> query = @this;
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            return query;
        }
    }
}
