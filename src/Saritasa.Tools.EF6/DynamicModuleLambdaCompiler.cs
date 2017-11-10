// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection;

namespace Saritasa.Tools.EF
{
    /// <summary>
    /// The class contains helper method to generate lambda for creating
    /// new object.
    /// </summary>
    internal static class DynamicModuleLambdaCompiler
    {
        /// <summary>
        /// Generate factory method.
        /// </summary>
        /// <remarks>
        /// Original: https://blogs.msdn.microsoft.com/seteplia/2017/02/01/dissecting-the-new-constraint-in-c-a-perfect-example-of-a-leaky-abstraction/.
        /// </remarks>
        /// <typeparam name="T">Type that implemented parameterless constructor.</typeparam>
        /// <returns>Delegate.</returns>
        internal static Func<T> GenerateFactory<T>() where T : new()
        {
            Expression<Func<T>> expr = () => new T();
            NewExpression newExpr = (NewExpression)expr.Body;

            var module = typeof(DynamicModuleLambdaCompiler).Module;
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
        }
    }
}
