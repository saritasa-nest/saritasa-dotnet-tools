// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Base middleware to resolve objects.
    /// </summary>
    public class BaseHandlerResolverMiddleware
    {
        /// <summary>
        /// Handle object key.
        /// </summary>
        public const string HandlerObjectKey = "handler-object";

        /// <summary>
        /// If <c>true</c> the middleware will resolve project using internal resolver. Default is <c>true</c>.
        /// </summary>
        public bool UseInternalObjectResolver { get; set; } = true;
    }
}
