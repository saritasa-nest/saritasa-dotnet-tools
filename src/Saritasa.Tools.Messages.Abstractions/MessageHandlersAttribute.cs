// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Abstractions
{
    using System;

    /// <summary>
    /// The attribuet indicates that class contains handlers. Useful to find all kind of
    /// handlers (commands, queries, etc).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageHandlersAttribute : Attribute
    {
    }
}
