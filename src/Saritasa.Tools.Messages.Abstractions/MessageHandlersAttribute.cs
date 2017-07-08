// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Abstractions
{
    /// <summary>
    /// The attribuet indicates that class contains handlers. Useful to find all kind of
    /// handlers (commands, queries, etc).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageHandlersAttribute : Attribute
    {
    }
}
