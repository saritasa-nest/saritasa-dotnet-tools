// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common.ObjectSerializers;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// JSON file target. Write JSON serialized messages separated by new line.
    /// </summary>
    /// <remarks>
    /// JSON processing utilities can be used to parse files. For example jl-sql (https://github.com/avz/jl-sql), jq.
    /// </remarks>
    public class JsonFileMessageRepository : IMessageRepository, IDisposable
    {
        /// <summary>
        /// Logs path.
        /// </summary>
        public string LogsPath { get; }

        FileStream currentFileStream;

        readonly IObjectSerializer serializer;

        readonly string prefix;

        readonly bool buffer;

        static readonly object objLock = new object();

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="logsPath">Logs path.</param>
        /// <param name="buffer">Should the output stream be buffered.</param>
        /// <param name="prefix">Files names prefix.</param>
        public JsonFileMessageRepository(string logsPath, string prefix = "", bool buffer = true)
        {
            if (string.IsNullOrEmpty(logsPath))
            {
                throw new ArgumentException(nameof(logsPath));
            }
            this.LogsPath = logsPath;
            this.serializer = new JsonObjectSerializer();
            this.prefix = prefix;
            this.buffer = buffer;
            Directory.CreateDirectory(LogsPath);
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        public JsonFileMessageRepository(IDictionary<string, string> dict)
        {
            this.LogsPath = dict["logsPath"].ToString();
            this.serializer = new JsonObjectSerializer();
            this.prefix = dict[nameof(prefix)].ToString();
            this.buffer = Convert.ToBoolean(dict[nameof(buffer)]);
            Directory.CreateDirectory(LogsPath);
        }

        string GetFileNameByDate(DateTime date, int count)
        {
            var name = $"{date:yyyyMMdd}-{count:000}.json";
            if (!string.IsNullOrEmpty(prefix))
            {
                name = prefix + "-" + name;
            }
            return name;
        }

        string GetAvailableFileNameByDate(DateTime date)
        {
            if (currentFileStream != null)
            {
                return Path.GetFileName(currentFileStream.Name);
            }

            return GetFileNameByDate(date, 0);
        }

        static readonly byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

        void WriteToFile(IMessage message)
        {
            var jsonBytes = serializer.Serialize(message.CloneToMessage());
            currentFileStream.Write(jsonBytes, 0, jsonBytes.Length);
            currentFileStream.Write(newLine, 0, newLine.Length);
        }

        #region IMessageRepository

        static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public Task AddAsync(IMessage message)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("The repository has been disposed.");
            }

            lock (objLock)
            {
                string name = GetAvailableFileNameByDate(DateTime.Now);
                if (currentFileStream == null || Path.GetFileName(currentFileStream.Name) != name)
                {
                    Close();
                    currentFileStream = new FileStream(Path.Combine(LogsPath, name), FileMode.Append);
                }
                WriteToFile(message);
            }

            if (!buffer)
            {
                lock (objLock)
                {
                    currentFileStream.FlushAsync();
                }
            }

            return completedTask;
        }

        /// <inheritdoc />
        public Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, string> dict)
        {
            dict["logsPath"] = LogsPath;
            dict[nameof(buffer)] = buffer.ToString();
            dict[nameof(prefix)] = prefix;
        }

        #endregion

        /// <summary>
        /// Close all streams.
        /// </summary>
        public void Close()
        {
            lock (objLock)
            {
                if (currentFileStream != null)
                {
                    currentFileStream.Dispose();
                    currentFileStream = null;
                }
            }
        }

        #region Dispose

        bool disposed;

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Close();
                }
                disposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
