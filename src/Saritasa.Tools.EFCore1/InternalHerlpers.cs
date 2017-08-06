// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace Saritasa.Tools.EFCore
{
    internal class InternalHelpers
    {
        internal static Task CompletedTask { get; } = Task.FromResult(1);
    }
}
