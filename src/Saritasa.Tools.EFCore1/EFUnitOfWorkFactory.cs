// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Saritasa.Tools.Domain;

namespace Saritasa.Tools.EFCore
{
    /// <summary>
    /// Unit of work factory implementation for Entity Framework Core. Note that it does not support
    /// to specify isolation level on creation.
    /// </summary>
    /// <typeparam name="TContext">Context type should be inherited of DbContext.</typeparam>
    public class EFUnitOfWorkFactory<TContext> : IUnitOfWorkFactory<EFUnitOfWork<TContext>>
        where TContext : DbContext, new()
    {
        private readonly Func<TContext> createContext;

        /// <summary>
        /// .ctor
        /// </summary>
        public EFUnitOfWorkFactory()
        {
            createContext = DynamicModuleLambdaCompiler.GenerateFactory<TContext>();
        }

        /// <inheritdoc />
        public virtual EFUnitOfWork<TContext> Create()
        {
            return new EFUnitOfWork<TContext>(createContext());
        }

        /// <inheritdoc />
        public virtual EFUnitOfWork<TContext> Create(IsolationLevel isolationLevel)
        {
            return new EFUnitOfWork<TContext>(createContext());
        }
    }
}
