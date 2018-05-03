// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Internal resolver options.
    /// </summary>
    public class InternalResolverOptions
    {
        /// <summary>
        /// If <c>true</c> the middleware resolves dependencies using internal resolver. Default is <c>true</c>.
        /// </summary>
        public bool UseInternalObjectResolver { get; set; } = true;

        /// <summary>
        /// Resolve handler object public properties using service provider. Default is <c>false</c>.
        /// </summary>
        public bool UsePropertiesResolving { get; set; } = false;

        /// <summary>
        /// If <c>true</c> the middleware will try to resolve executing method parameters. Default is <c>true</c>.
        /// </summary>
        public bool UseHandlerParametersResolve { get; set; } = true;
    }
}
