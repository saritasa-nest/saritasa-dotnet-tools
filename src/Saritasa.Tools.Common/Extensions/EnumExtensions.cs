// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using Saritasa.Tools.Common.Utils;

namespace Saritasa.Tools.Common.Extensions
{
    /// <summary>
    /// System.Enum extensions.
    /// </summary>
    public static class EnumExtensions
    {
#if !PORTABLE && !NETSTANDARD1_2 && !NETSTANDARD1_6 && !NETCOREAPP1_0 && !NETCOREAPP1_1
        /// <summary>
        /// Gets the value of Description attribute.
        /// </summary>
        /// <param name="target">Enum.</param>
        /// <returns>Description text.</returns>
        public static string GetDescription(this Enum target)
        {
            return EnumUtils.GetDescription(target);
        }
#endif

        /// <summary>
        /// Gets the custom attribute of enum.
        /// </summary>
        /// <param name="target">Enum.</param>
        /// <typeparam name="TAttribute">Attribute type.</typeparam>
        /// <returns>Attribute or null if not found.</returns>
        public static TAttribute GetAttribute<TAttribute>(this Enum target)
            where TAttribute : Attribute
        {
            return EnumUtils.GetAttribute<TAttribute>(target);
        }
    }
}
