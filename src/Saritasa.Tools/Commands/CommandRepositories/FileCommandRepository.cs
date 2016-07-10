// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Commands.CommandRepositories
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using Internal;

    /// <summary>
    /// Makes log of all executed commands.
    /// </summary>
    public class FileCommandRepository : ICommandRepository, IDisposable
    {
        /// <summary>
        /// Logs path.
        /// </summary>
        public string LogsPath { get; private set; }

        bool disposed;

        FileStream currentFileStream;

        GZipStream currentGZipStream;

        CommandBinarySerializer currentBinarySerializer;

        readonly IObjectSerializer objectSerializer;

        readonly string prefix;

        readonly bool buffer;

        readonly bool compress;

        readonly object objLock = new object();

        Stream CurrentStream
        {
            get
            {
                return currentGZipStream != null ? currentGZipStream : (Stream)currentFileStream;
            }
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="logsPath">Logs path.</param>
        /// <param name="serializer">Object serializer.</param>
        /// <param name="buffer">Should the output stream be buffered.</param>
        /// <param name="compress">Compress target files.</param>
        /// <param name="prefix">Files names prefix.</param>
        public FileCommandRepository(string logsPath, IObjectSerializer serializer, string prefix = "",
            bool buffer = true, bool compress = false)
        {
            if (!Directory.Exists(logsPath))
            {
                throw new ArgumentException($"Directory {logsPath} does not exist", nameof(logsPath));
            }
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            LogsPath = logsPath;
            objectSerializer = serializer;
            this.prefix = prefix;
            this.buffer = buffer;
            this.compress = compress;
            Directory.CreateDirectory(LogsPath);
        }

        private string GetFileNameByDate(DateTime date)
        {
            var name = $"{date.ToString("yyyyMMdd")}.bin";
            if (string.IsNullOrEmpty(prefix) == false)
            {
                name = prefix + "-" + name;
            }
            if (compress)
            {
                name += ".zip";
            }
            return name;
        }

        #region ICommandRepository

        /// <inheritdoc />
        public void Add(CommandExecutionResult context)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("The command repository has been disposed.");
            }

            string name = GetFileNameByDate(DateTime.Now);
            lock (objLock)
            {
                if (currentFileStream == null || Path.GetFileName(currentFileStream.Name) != name)
                {
                    Close();
                    currentFileStream = new FileStream(Path.Combine(LogsPath, name), FileMode.Append);
                    if (compress)
                    {
                        currentGZipStream = new GZipStream(currentFileStream, CompressionMode.Compress);
                    }
                    currentBinarySerializer = new CommandBinarySerializer(CurrentStream, objectSerializer);
                }
            }

            currentBinarySerializer.Write(context);
            if (!buffer)
            {
                lock (objLock)
                {
                    if (currentGZipStream != null)
                    {
                        currentGZipStream.Flush();
                    }
                    currentFileStream.FlushAsync();
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<CommandExecutionResult> GetByDates(DateTime startDate, DateTime endDate)
        {
            var allFiles = new HashSet<string>(Directory.GetFiles(LogsPath).Select(f => Path.GetFileName(f)));
            var targetList = new List<CommandExecutionResult>(150);
            DateTime currentDate = startDate;
            while (currentDate <= endDate)
            {
                var fileName = GetFileNameByDate(currentDate);
                if (allFiles.Contains(fileName) == false)
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                Stream stream = null;
                try
                {
                    stream = new FileStream(Path.Combine(LogsPath, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    if (compress)
                    {
                        stream = new GZipStream(stream, CompressionMode.Decompress, false);
                    }
                    var commandSerializer = new CommandBinarySerializer(stream, objectSerializer);
                    for (CommandExecutionResult cmdResult = null; (cmdResult = commandSerializer.Read()) != null;)
                    {
                        if (cmdResult.CreatedAt >= startDate && cmdResult.CreatedAt <= endDate)
                        {
                            targetList.Add(cmdResult);
                        }
                    }
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Dispose();
                    }
                }
                currentDate = currentDate.AddDays(1);
            }
            return targetList;
        }

        /// <summary>
        /// Close all streams.
        /// </summary>
        public void Close()
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

        #endregion

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
