// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Queries
{
    using System;
    using Messages;

    /// <summary>
    /// Queries specific pipeline.
    /// </summary>
    public interface IQueryPipeline : IMessagePipeline
    {
        /// <summary>
        /// Executes query with no input parameters.
        /// </summary>
        /// <typeparam name="TResult">Query result.</typeparam>
        /// <param name="func">Query function.</param>
        /// <returns>Result.</returns>
        TResult Execute<TResult>(Func<TResult> func);

        /// <summary>
        /// Executes query with one input parameter.
        /// </summary>
        /// <typeparam name="TResult">Query result.</typeparam>
        /// <typeparam name="T">Argument type.</typeparam>
        /// <param name="func">Query function.</param>
        /// <param name="arg">Argument.</param>
        /// <returns>Result.</returns>
        TResult Execute<T, TResult>(Func<T, TResult> func, T arg);

        /// <summary>
        /// Executes query with 2 input parameters.
        /// </summary>
        /// <typeparam name="TResult">Query result.</typeparam>
        /// <typeparam name="T1">Argument 1 type.</typeparam>
        /// <typeparam name="T2">Argument 2 type.</typeparam>
        /// <param name="func">Query function.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <returns>Result.</returns>
        TResult Execute<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 arg1, T2 arg2);

        /// <summary>
        /// Executes query with 3 input parameters.
        /// </summary>
        /// <typeparam name="TResult">Query result.</typeparam>
        /// <typeparam name="T1">Argument 1 type.</typeparam>
        /// <typeparam name="T2">Argument 2 type.</typeparam>
        /// <typeparam name="T3">Argument 3 type.</typeparam>
        /// <param name="func">Query function.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <returns>Result.</returns>
        TResult Execute<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 arg1, T2 arg2, T3 arg3);

        /// <summary>
        /// Executes query with 4 input parameters.
        /// </summary>
        /// <typeparam name="TResult">Query result.</typeparam>
        /// <typeparam name="T1">Argument 1 type.</typeparam>
        /// <typeparam name="T2">Argument 2 type.</typeparam>
        /// <typeparam name="T3">Argument 3 type.</typeparam>
        /// <typeparam name="T4">Argument 4 type.</typeparam>
        /// <param name="func">Query function.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <returns>Result.</returns>
        TResult Execute<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4);

        /// <summary>
        /// Executes query with 5 input parameters.
        /// </summary>
        /// <typeparam name="TResult">Query result.</typeparam>
        /// <typeparam name="T1">Argument 1 type.</typeparam>
        /// <typeparam name="T2">Argument 2 type.</typeparam>
        /// <typeparam name="T3">Argument 3 type.</typeparam>
        /// <typeparam name="T4">Argument 4 type.</typeparam>
        /// <typeparam name="T5">Argument 5 type.</typeparam>
        /// <param name="func">Query function.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <returns>Result.</returns>
        TResult Execute<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        /// <summary>
        /// Executes query with 6 input parameters.
        /// </summary>
        /// <typeparam name="TResult">Query result.</typeparam>
        /// <typeparam name="T1">Argument 1 type.</typeparam>
        /// <typeparam name="T2">Argument 2 type.</typeparam>
        /// <typeparam name="T3">Argument 3 type.</typeparam>
        /// <typeparam name="T4">Argument 4 type.</typeparam>
        /// <typeparam name="T5">Argument 5 type.</typeparam>
        /// <typeparam name="T6">Argument 6 type.</typeparam>
        /// <param name="func">Query function.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <returns>Result.</returns>
        TResult Execute<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

        /// <summary>
        /// Executes query with 7 input parameters.
        /// </summary>
        /// <typeparam name="TResult">Query result.</typeparam>
        /// <typeparam name="T1">Argument 1 type.</typeparam>
        /// <typeparam name="T2">Argument 2 type.</typeparam>
        /// <typeparam name="T3">Argument 3 type.</typeparam>
        /// <typeparam name="T4">Argument 4 type.</typeparam>
        /// <typeparam name="T5">Argument 5 type.</typeparam>
        /// <typeparam name="T6">Argument 6 type.</typeparam>
        /// <typeparam name="T7">Argument 7 type.</typeparam>
        /// <param name="func">Query function.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <param name="arg7">Argument 7.</param>
        /// <returns>Result.</returns>
        TResult Execute<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

        /// <summary>
        /// Executes query with 8 input parameters.
        /// </summary>
        /// <typeparam name="TResult">Query result.</typeparam>
        /// <typeparam name="T1">Argument 1 type.</typeparam>
        /// <typeparam name="T2">Argument 2 type.</typeparam>
        /// <typeparam name="T3">Argument 3 type.</typeparam>
        /// <typeparam name="T4">Argument 4 type.</typeparam>
        /// <typeparam name="T5">Argument 5 type.</typeparam>
        /// <typeparam name="T6">Argument 6 type.</typeparam>
        /// <typeparam name="T7">Argument 7 type.</typeparam>
        /// <typeparam name="T8">Argument 8 type.</typeparam>
        /// <param name="func">Query function.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <param name="arg4">Argument 4.</param>
        /// <param name="arg5">Argument 5.</param>
        /// <param name="arg6">Argument 6.</param>
        /// <param name="arg7">Argument 7.</param>
        /// <param name="arg8">Argument 8.</param>
        /// <returns>Result.</returns>
        TResult Execute<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

        /// <summary>
        /// Get query object. Needed to resolve delegate, dependencies will not be resolved.
        /// </summary>
        /// <typeparam name="TQuery">Query type.</typeparam>
        /// <returns>Transient query object.</returns>
        TQuery GetQuery<TQuery>() where TQuery : class;
    }
}
