// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Internal;
using Saritasa.Tools.Messages.Common.ObjectSerializers;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// Store messages to files.
    /// </summary>
    public class FileMessageRepository : BaseFileRepository, IMessageRepository, IDisposable
    {
        private const string KeyCompress = "compress";

        private bool disposed;

        private FileStream currentFileStream;

        private GZipStream currentGZipStream;

        private MessageBinarySerializer currentBinarySerializer;

        private readonly bool compress;

        private static readonly object objLock = new object();

        private Stream CurrentStream => currentGZipStream ?? (Stream)currentFileStream;

        /// <summary>
        /// Current file name extension.
        /// </summary>
        public override string FileNameExtension => compress ? ".bin.zip" : ".bin";

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="path">Logs path.</param>
        /// <param name="serializer">Object serializer. By default json serializer is used.</param>
        /// <param name="buffer">Should the output stream be buffered.</param>
        /// <param name="compress">Compress target files.</param>
        /// <param name="prefix">Files names prefix.</param>
        public FileMessageRepository(
            string path,
            IObjectSerializer serializer = null,
            string prefix = "",
            bool buffer = true,
            bool compress = false)
        {
            this.Path = path;
            this.Serializer = serializer ?? new JsonObjectSerializer();
            this.FileNamePrefix = prefix;
            this.BufferStream = buffer;
            this.compress = compress;
            ValidateAndInit();
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="parameters">Parameters dictionary..</param>
        public FileMessageRepository(IDictionary<string, string> parameters) : base(parameters)
        {
            this.compress = Convert.ToBoolean(parameters.GetValueOrDefault(KeyCompress, false.ToString()));
            ValidateAndInit();
        }

        #region IMessageRepository

        static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public Task AddAsync(IMessage context, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            lock (objLock)
            {
                // We cannot continue zip streams, so we have to create new file
                // every time with new stream.
                string name = GetAvailableFileNameByDate(currentFileStream, DateTime.Now, compress);
                if (currentFileStream == null || System.IO.Path.GetFileName(currentFileStream.Name) != name)
                {
                    Close();
                    currentFileStream = new FileStream(System.IO.Path.Combine(Path, name), FileMode.Append);
                    if (compress)
                    {
                        currentGZipStream = new GZipStream(currentFileStream, CompressionMode.Compress);
                    }
                    currentBinarySerializer = new MessageBinarySerializer(CurrentStream, Serializer, null);
                }
                currentBinarySerializer.Write(context);
            }

            if (!BufferStream)
            {
                lock (objLock)
                {
                    currentGZipStream?.Flush();
                    currentFileStream.Flush();
                }
            }

            return completedTask;
        }

        /// <inheritdoc />
        public override void SaveState(IDictionary<string, string> parameters)
        {
            base.SaveState(parameters);
            parameters[KeyCompress] = compress.ToString();
        }

        #endregion

        /// <inheritdoc />
        protected override IEnumerable<IMessage> ReadMessagesFromStream(Stream stream,
            MessageQuery query)
        {
            if (compress)
            {
                stream = new GZipStream(stream, CompressionMode.Decompress, false);
            }
            var commandSerializer = new MessageBinarySerializer(stream, Serializer, query.Assemblies.ToArray());
            for (Message message; (message = commandSerializer.Read()) != null;)
            {
                if (query.Match(message))
                {
                    yield return message;
                }
            }
        }

        /// <summary>
        /// Close all streams.
        /// </summary>
        public void Close()
        {
            lock (objLock)
            {
                if (currentGZipStream != null)
                {
                    currentGZipStream.Dispose();
                    currentGZipStream = null;
                }
                if (currentFileStream != null)
                {
                    currentFileStream.Dispose();
                    currentFileStream = null;
                }
            }
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (!disposing)
            {
                Close();
            }
            disposed = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
