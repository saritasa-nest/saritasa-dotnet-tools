// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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
    public class JsonFileMessageRepository : BaseFileRepository, IMessageRepository, IDisposable
    {
        private FileStream currentFileStream;

        /// <inheritdoc />
        public override string FileNameExtension => ".json";

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="path">Logs path.</param>
        /// <param name="buffer">Should the output stream be buffered.</param>
        /// <param name="prefix">Files names prefix.</param>
        public JsonFileMessageRepository(string path, string prefix = "", bool buffer = true)
        {
            this.Path = path;
            this.FileNamePrefix = prefix;
            this.BufferStream = buffer;
            ValidateAndInit();
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        public JsonFileMessageRepository(IDictionary<string, string> parameters) : base(parameters)
        {
            ValidateAndInit();
        }

        /// <inheritdoc />
        protected override void ValidateAndInit()
        {
            if (!(this.Serializer is JsonObjectSerializer))
            {
                this.Serializer = new JsonObjectSerializer();
            }
            base.ValidateAndInit();
        }

        private static readonly byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

        private void WriteToFile(MessageRecord message)
        {
            var jsonBytes = Serializer.Serialize(message);
            currentFileStream.Write(jsonBytes, 0, jsonBytes.Length);
            currentFileStream.Write(newLine, 0, newLine.Length);
        }

        #region IMessageRepository

        static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public Task AddAsync(MessageRecord messageRecord, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("The repository has been disposed.");
            }

            lock (SyncRoot)
            {
                string name = GetAvailableFileNameByDate(currentFileStream, DateTime.Now);
                if (currentFileStream == null || System.IO.Path.GetFileName(currentFileStream.Name) != name)
                {
                    Close();
                    currentFileStream = new FileStream(System.IO.Path.Combine(Path, name), FileMode.Append);
                }
                WriteToFile(messageRecord);
            }

            if (!BufferStream)
            {
                lock (SyncRoot)
                {
                    currentFileStream.Flush();
                }
            }

            return completedTask;
        }

        #endregion

        /// <inheritdoc />
        protected override IEnumerable<MessageRecord> ReadMessagesFromStream(Stream stream,
            MessageQuery query)
        {
            return new List<MessageRecord>();
        }

        /// <summary>
        /// Close all streams.
        /// </summary>
        public void Close()
        {
            lock (SyncRoot)
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
