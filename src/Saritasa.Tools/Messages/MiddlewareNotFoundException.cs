// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages
{
    using System;

    /// <summary>
    /// Middleware not found exception.
    /// </summary>
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class MiddlewareNotFoundException : Exception
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public MiddlewareNotFoundException() : base("Middleware not found")
        {
        }
    }
}
