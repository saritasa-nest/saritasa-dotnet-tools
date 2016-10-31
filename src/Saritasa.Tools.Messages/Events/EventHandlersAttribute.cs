// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Events
{
    using System;
    using Common;

    /// <summary>
    /// The class that contains commands handers should be marked
    /// with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class EventHandlersAttribute : MessageHandlersAttribute
    {
    }
}
