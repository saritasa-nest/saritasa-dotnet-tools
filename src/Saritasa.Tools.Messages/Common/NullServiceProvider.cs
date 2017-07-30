// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Service provider that always resolves to null.
    /// </summary>
    public class NullServiceProvider : IServiceProvider
    {
        /// <summary>
        /// Default static instance.
        /// </summary>
        public static readonly NullServiceProvider Default = new NullServiceProvider();

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            return null;
        }
    }
}
