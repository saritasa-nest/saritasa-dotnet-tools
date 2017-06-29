// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Object utils.
    /// </summary>
    public static class ObjectUtils
    {
        /// <summary>
        /// Generate factory method. For example new() generic constraint uses <see cref="Activator" />
        /// for creating new instances. It can be optimized by making factory delegate.
        /// </summary>
        /// <remarks>
        /// Original: https://blogs.msdn.microsoft.com/seteplia/2017/02/01/dissecting-the-new-constraint-in-c-a-perfect-example-of-a-leaky-abstraction/.
        /// </remarks>
        /// <typeparam name="T">Type that implemented parameterless constructor.</typeparam>
        /// <returns>Factory delegate.</returns>
        public static Func<T> CreateTypeFactory<T>() where T : new()
        {
#if !PORTABLE && !NETSTANDARD1_2 && !NETSTANDARD1_6 && !NETCOREAPP1_0 && !NETCOREAPP1_1
            Expression<Func<T>> expr = () => new T();
            NewExpression newExpr = (NewExpression)expr.Body;

            var module = typeof(ObjectUtils).Module;
            var method = new DynamicMethod(
                name: "LambdaFor" + typeof(T).Name,
                returnType: newExpr.Type,
                parameterTypes: new Type[0],
                m: module,
                skipVisibility: true);

            ILGenerator ilGen = method.GetILGenerator();

            // Constructor for value types could be null.
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (newExpr.Constructor != null)
            {
                ilGen.Emit(OpCodes.Newobj, newExpr.Constructor);
            }
            else
            {
                LocalBuilder temp = ilGen.DeclareLocal(newExpr.Type);
                ilGen.Emit(OpCodes.Ldloca, temp);
                ilGen.Emit(OpCodes.Initobj, newExpr.Type);
                ilGen.Emit(OpCodes.Ldloc, temp);
            }
            ilGen.Emit(OpCodes.Ret);

            return (Func<T>)method.CreateDelegate(typeof(Func<T>));
#else
            return Activator.CreateInstance<T>;
#endif
        }
    }
}
