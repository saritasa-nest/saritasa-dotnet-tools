// Copyright (c) 2015-2023, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if NET40
// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    /// <summary>
    /// Type extensions for .NET 4.0 compatibility.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Retrieves the type information.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>Type information.</returns>
        internal static TypeInfo GetTypeInfo(this Type type)
        {
            return new TypeInfo(type);
        }
    }
}
#endif
