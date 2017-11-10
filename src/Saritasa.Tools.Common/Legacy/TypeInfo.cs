// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

#if NET40
// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    /// <summary>
    /// Represents type declarations for class types, interface types, array types, value types,
    /// enumeration types, type parameters, generic type definitions, and open or closed constructed generic types.
    /// Here is used for .NET 4.0 compatibility.
    /// </summary>
    public class TypeInfo
    {
        readonly Type type;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="type">Wrapped type.</param>
        public TypeInfo(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            this.type = type;
        }

        /// <summary>
        /// Assembly type is related to.
        /// </summary>
        public Assembly Assembly => type.Assembly;
    }
}
#endif
