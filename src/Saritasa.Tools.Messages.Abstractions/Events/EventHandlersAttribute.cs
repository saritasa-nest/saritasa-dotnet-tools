// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Abstractions.Events
{
    /// <summary>
    /// Marks the class that it contains events handlers.
    /// Otherwise handlers will not be found for events.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EventHandlersAttribute : Attribute
    {
    }
}
