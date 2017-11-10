// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Service provider that is based on function.
    /// </summary>
    public class FuncServiceProvider : IServiceProvider
    {
        private readonly Func<Type, object> resolverFunc;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolverFunc">Resolver function.</param>
        public FuncServiceProvider(Func<Type, object> resolverFunc)
        {
            this.resolverFunc = resolverFunc;
        }

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            return resolverFunc(serviceType);
        }
    }
}
