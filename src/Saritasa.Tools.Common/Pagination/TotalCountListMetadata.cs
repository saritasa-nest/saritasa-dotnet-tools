// Copyright (c) 2015-2023, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Common.Pagination;

/// <summary>
/// Metadata class with the total count of items.
/// </summary>
#if NET40 || NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
[Serializable]
#endif
public class TotalCountListMetadata
{
    /// <summary>
    /// The total count of items.
    /// </summary>
    public int TotalCount { get; set; }
}
