// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.EntityFramework
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Validation;
    using System.Linq;
    using Domain;

    /// <summary>
    /// Entity framework implementation for ISession.
    /// </summary>
    public class EfSession<TContext> : IUnitOfWork
        where TContext : DbContext, new()
    {
        private TContext context;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="context">Database context.</param>
        public EfSession(TContext context)
        {
            this.context = context;
        }

        /// <inheritdoc />
        public IQueryable<TEntity> GetAll<TEntity>(string path = "") where TEntity : class
        {
            var set = (System.Data.Entity.Infrastructure.DbQuery<TEntity>)context.Set<TEntity>();
            if (string.IsNullOrEmpty(path) == false)
            {
                var includes = path.Split(new char[] { ',' });
                foreach (var include in includes)
                {
                    set = set.Include(include.Trim());
                }
            }
            return set;
        }

        /// <inheritdoc />
        public TEntity Get<TEntity>(object id) where TEntity : class
        {
            return context.Set<TEntity>().Find(id);
        }

        /// <inheritdoc />
        public void MarkAdded<TEntity>(TEntity entity) where TEntity : class
        {
            context.Set<TEntity>().Add(entity);
        }

        /// <inheritdoc />
        public void MarkRemoved<TEntity>(TEntity entity) where TEntity : class
        {
            context.Set<TEntity>().Remove(entity);
        }

        /// <inheritdoc />
        public void Attach<TEntity>(TEntity entity) where TEntity : class
        {
            context.Set<TEntity>().Attach(entity);
        }

        /// <inheritdoc />
        public void Commit()
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException validationException)
            {
                throw new Exception(
                    string.Join(", ", validationException.EntityValidationErrors.Select(x => x.ValidationErrors.First().ErrorMessage)));
            }
        }

        #region Dispose

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            context.Dispose();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
