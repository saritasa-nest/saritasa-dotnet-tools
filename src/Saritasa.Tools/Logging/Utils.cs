// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Logging
{
    using System;
    using System.Diagnostics;
#if !NETCOREAPP1_0 && !NETSTANDARD1_6
    using System.Reflection;
#else
    using System.IO;
#endif
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Various logging utils.
    /// </summary>
    public static class Utils
    {
#if NETCOREAPP1_0 || NETSTANDARD1_6
        /// <summary>
        /// Creates the logger with fully qualified name of the current file name.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ILogger GetCurrentClassLogger(ILoggerFactory factory, [CallerFilePath] string path = "")
        {
            var filename = Path.GetFileNameWithoutExtension(path);
            return factory.GetLogger(filename);
        }
#else
        /// <summary>
        /// Creates the logger with fully qualified name of the current class, including the 
        /// namespace but not the assembly.
        /// </summary>
        /// <remarks>
        /// Implementation has been gotten from https://github.com/NLog/NLog/blob/master/src/NLog/LogManager.cs
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ILogger GetCurrentClassLogger(ILoggerFactory factory)
        {
            string className;
            Type declaringType;
            int framesToSkip = 1;

            do
            {
                var frame = new StackFrame(framesToSkip, false);
                var method = frame.GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    className = method.Name;
                    break;
                }

                framesToSkip++;
                className = declaringType.FullName;
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return factory.GetLogger(className);
        }
#endif
    }
}
