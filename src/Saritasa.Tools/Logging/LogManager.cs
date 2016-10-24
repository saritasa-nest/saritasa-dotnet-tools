// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Logging
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Creates and manages instances of loggers. By default DummyLoggerFactory is used.
    /// </summary>
    public static class LogManager
    {
        static ILoggerFactory factory = new DummyLoggerFactory();

        /// <summary>
        /// Set default logger factory instance.
        /// </summary>
        /// <param name="factory">Logger factory instance.</param>
        public static void SetFactory(ILoggerFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            LogManager.factory = factory;
        }

        /// <summary>
        /// Gets the specified name logger.
        /// </summary>
        /// <param name="name">Logger name.</param>
        /// <returns>Named logger.</returns>
        public static ILogger GetLogger(string name)
        {
            return factory.GetLogger(name);
        }

        /// <summary>
        /// Get the logger with name of current class.
        /// </summary>
        /// <returns>Named logger.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ILogger GetCurrentClassLogger()
        {
            return Utils.GetCurrentClassLogger(factory);
        }

        #region Temporary file logger

        static object templock = new object();

        static string tempFileName = string.Empty;

        /// <summary>
        /// Sometimes you need very simple temporary logging without any configuration. That is when LogTemp
        /// method can be used. All messages will be stored to C:\Users\UserName\AppData\Local\Temp\SaritasaTools.LogManager .
        /// Thread safe.
        /// </summary>
        /// <param name="message">Log message.</param>
        public static void LogTemp(string message)
        {
            lock (templock)
            {
                // if first call init directory and file
                if (string.IsNullOrEmpty(tempFileName))
                {
                    var tempDir = Path.Combine(Path.GetTempPath(), "SaritasaTools.LogManager");
                    if (Directory.Exists(tempDir) == false)
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    tempFileName = Path.Combine(tempDir, DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".log");
                }

                // log
                File.AppendAllText(tempFileName, message + Environment.NewLine);
            }
        }

        #endregion
    }
}
