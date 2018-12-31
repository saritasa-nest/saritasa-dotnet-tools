// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Abstractions.Commands
{
    /// <summary>
    /// Marks command property as output. Not for processing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CommandOutAttribute : Attribute
    {
    }
}
