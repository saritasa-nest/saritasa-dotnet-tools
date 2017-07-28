// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Reflection;

namespace Saritasa.Tools.Messages.Events.PipelineMiddlewares
{
    /// <summary>
    /// Structure represents <see cref="MethodInfo"/> handler for event
    /// and resolved object to execute on.
    /// </summary>
    public struct EventHandlerMethodWithObject
    {
        /// <summary>
        /// Method.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// Object to invoke on.
        /// </summary>
        public object Object { get; }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="method">Method info.</param>
        /// <param name="obj">Object invoke on.</param>
        public EventHandlerMethodWithObject(MethodInfo method, object obj = null)
        {
            this.Method = method;
            this.Object = obj;
        }
    }
}
