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
        /// <typeparam name="T1">Argument type.</typeparam>
        /// <typeparam name="T2">Argument type.</typeparam>
        /// <param name="func">Query function.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <returns>Result.</returns>
        TResult Execute<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 arg1, T2 arg2);

        /// <summary>
        /// Executes query with 2 input parameters.
        /// </summary>
        /// <typeparam name="TResult">Query result.</typeparam>
        /// <typeparam name="T1">Argument type.</typeparam>
        /// <typeparam name="T2">Argument type.</typeparam>
        /// <typeparam name="T3">Argument type.</typeparam>
        /// <param name="func">Query function.</param>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        /// <param name="arg3">Argument 3.</param>
        /// <returns>Result.</returns>
        TResult Execute<T1, T2, T3, TResult>(Func<T1, T2, TResult> func, T1 arg1, T2 arg2, T3 arg3);
    }
}
