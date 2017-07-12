// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Saritasa.Tools.Messages.Internal;
using Saritasa.Tools.Messages.Common.ObjectSerializers;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// Base file repository for file, csv, json formats.
    /// </summary>
    public abstract class BaseFileRepository
    {
        private const string KeyPath = "path";
        private const string KeyPrefix = "prefix";
        private const string KeyBuffer = "buffer";
        private const string KeySerializer = "serializer";

        /// <summary>
        /// Date format used for file name.
        /// </summary>
        public const string DateTimeFormat = "yyyyMMdd";

        /// <summary>
        /// Current file name prefix.
        /// </summary>
        public string FileNamePrefix { get; protected set; }

        /// <summary>
        /// Current file name extension.
        /// </summary>
        public abstract string FileNameExtension { get; }

        /// <summary>
        /// Serializer used.
        /// </summary>
        public IObjectSerializer Serializer { get; protected set; }

        /// <summary>
        /// Logs path.
        /// </summary>
        public string Path { get; protected set; }

        /// <summary>
        /// Should file stream be buffered. <c>false</c> by default.
        /// </summary>
        public bool BufferStream { get; protected set; }

        /// <summary>
        /// .ctor
        /// </summary>
        protected BaseFileRepository()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="parameters">Dictionary parameters.</param>
        protected BaseFileRepository(IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            Path = parameters.GetValueOrInvoke(KeyPath, RepositoryConfigurationException.ThrowParameterNotExists);
            FileNamePrefix = parameters.GetValueOrInvoke(KeyPrefix, RepositoryConfigurationException.ThrowParameterNotExists);
            BufferStream = Convert.ToBoolean(parameters.GetValueOrDefault(KeyBuffer, false.ToString()));
            if (parameters.ContainsKey(KeySerializer))
            {
                Serializer = (IObjectSerializer)Activator.CreateInstance(
                    Type.GetType(parameters[KeySerializer]));
            }
        }

        /// <summary>
        /// Validate repository and init if needed.
        /// </summary>
        protected virtual void ValidateAndInit()
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new RepositoryConfigurationException("Path should not be empty.");
            }
            Directory.CreateDirectory(Path);
            if (Serializer == null)
            {
                Serializer = new JsonObjectSerializer();
            }
        }

        /// <summary>
        /// Save repository state to dictionary.
        /// </summary>
        /// <param name="parameters">Parameters dicitonary.</param>
        public virtual void SaveState(IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            parameters[KeyPath] = Path;
            parameters[KeyPrefix] = FileNamePrefix;
            parameters[KeyBuffer] = BufferStream.ToString();
            parameters[KeySerializer] = Serializer.GetType().AssemblyQualifiedName;
        }

        /// <summary>
        /// Get file name. For example tmp-20170712-000.json .
        /// </summary>
        /// <param name="date">Date.</param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected string GetFileNameByDate(DateTime date, int count)
        {
            var name = $"{date:yyyyMMdd}-{count:000}{FileNameExtension}";
            if (!string.IsNullOrEmpty(FileNamePrefix))
            {
                name = FileNamePrefix + "-" + name;
            }
            return name;
        }

        DateTime currentDate = DateTime.MinValue;

        /// <summary>
        /// Get file name that is available now and relates to date.
        /// </summary>
        /// <param name="currentFileStream">Current file stream.
        /// The method would try to get name from it first.</param>
        /// <param name="date">Date for file.</param>
        /// <param name="useCounter">If counter should be used the new file will be
        /// created for every new stream.</param>
        /// <returns>File name.</returns>
        protected string GetAvailableFileNameByDate(
            FileStream currentFileStream,
            DateTime date,
            bool useCounter = false)
        {
            if (date.Date == currentDate && currentFileStream != null)
            {
                return System.IO.Path.GetFileName(currentFileStream.Name);
            }

            currentDate = date.Date;
            string name = string.Empty;
            for (int i = 0; i < 1000; i++)
            {
                name = GetFileNameByDate(date, i);

                if (useCounter && currentFileStream == null && File.Exists(System.IO.Path.Combine(Path, name)))
                {
                    continue;
                }
                break;
            }
            return name;
        }
    }
}
