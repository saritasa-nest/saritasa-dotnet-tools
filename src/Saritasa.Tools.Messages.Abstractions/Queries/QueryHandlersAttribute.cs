// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Abstractions.Queries
{
    /// <summary>
    /// Marks the class that it contains query handlers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class QueryHandlersAttribute : Attribute
    {
    }
}
