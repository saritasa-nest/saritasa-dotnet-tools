// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
#if NETSTANDARD1_2
using System.Reflection;
#endif

namespace Saritasa.Tools.Common.Utils
{
    /// <summary>
    /// Provides methods to control execution flow.
    /// </summary>
    public static partial class FlowUtils
    {
        /// <summary>
        /// It is null-safe and thread-safe way to raise event. The method calls every handler related to event regardless thrown exceptions.
        /// If any handler throws an error the <see cref="System.AggregateException" /> will be thrown.
        /// </summary>
        public static void RaiseAll<TEventArgs>(object sender, TEventArgs e, ref EventHandler<TEventArgs> eventDelegate)
#if NET40 || NET452 || NET461
            where TEventArgs : EventArgs
#endif
        {
#if NETSTANDARD1_2
            var temp = Volatile.Read(ref eventDelegate);
#else
            var temp = eventDelegate;
#endif
#if NET40 || NET452 || NET461
            Thread.MemoryBarrier();
#endif
            if (temp == null)
            {
                return;
            }

            var exceptions = new List<Exception>();
            foreach (var handler in temp.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(sender, e);
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    exceptions.Add(ex.InnerException);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}
