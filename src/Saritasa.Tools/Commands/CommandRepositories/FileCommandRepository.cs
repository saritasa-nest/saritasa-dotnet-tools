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
    using ObjectSerializers;

    /// <summary>
    /// Makes log of all executed commands.
    /// </summary>
    public class FileCommandRepository : ICommandRepository, ICommandRepositoryPersist, IDisposable
    {
        /// <summary>
        /// Logs path.
        /// </summary>
        public string LogsPath
        {
            get { return logsPath; }
        }

        bool disposed;

        FileStream currentFileStream;

        GZipStream currentGZipStream;

        CommandBinarySerializer currentBinarySerializer;

        readonly string logsPath = string.Empty;

        readonly IObjectSerializer serializer;

        readonly string prefix;

        readonly bool buffer;

        readonly bool compress;

        static readonly object ObjLock = new object();

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
        /// <param name="serializer">Object serializer. By default json serializer is used.</param>
        /// <param name="buffer">Should the output stream be buffered.</param>
        /// <param name="compress">Compress target files.</param>
        /// <param name="prefix">Files names prefix.</param>
        public FileCommandRepository(string logsPath, IObjectSerializer serializer = null, string prefix = "",
            bool buffer = true, bool compress = false)
        {
            if (string.IsNullOrEmpty(logsPath))
            {
                throw new ArgumentException(nameof(logsPath));
            }
            this.logsPath = logsPath;
            this.serializer = serializer != null ? serializer : new JsonObjectSerializer();
            this.prefix = prefix;
            this.buffer = buffer;
            this.compress = compress;
            Directory.CreateDirectory(LogsPath);
        }

        private string GetFileNameByDate(DateTime date, int count)
        {
            var name = $"{date:yyyyMMdd}-{count:000}.bin";
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

        private string GetAvailableFileNameByDate(DateTime date)
        {
            if (currentFileStream != null)
            {
                return Path.GetFileName(currentFileStream.Name);
            }

            string name = string.Empty;
            for (int i = 0; i < 1000; i++)
            {
                name = GetFileNameByDate(date, i);

                // we cannot continue zip streams, so we have to create new file
                // every time with new stream
                if (compress && File.Exists(Path.Combine(LogsPath, name)))
                {
                    continue;
                }
                else
                {
                    break;
                }
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

            lock (ObjLock)
            {
                string name = GetAvailableFileNameByDate(DateTime.Now);
                if (currentFileStream == null || Path.GetFileName(currentFileStream.Name) != name)
                {
                    Close();
                    currentFileStream = new FileStream(Path.Combine(LogsPath, name), FileMode.Append);
                    if (compress)
                    {
                        currentGZipStream = new GZipStream(currentFileStream, CompressionMode.Compress);
                    }
                    currentBinarySerializer = new CommandBinarySerializer(CurrentStream, serializer);
                }
            }

            currentBinarySerializer.Write(context);
            if (!buffer)
            {
                lock (ObjLock)
                {
                    if (currentGZipStream != null)
                    {
                        currentGZipStream.Flush();
                    }
                    currentFileStream.Flush();
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
                for (int i = 0; i < 1000; i++)
                {
                    var fileName = GetFileNameByDate(currentDate, i);
                    if (allFiles.Contains(fileName) == false)
                    {
                        break;
                    }

                    Stream stream = null;
                    try
                    {
                        stream = new FileStream(Path.Combine(LogsPath, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        if (compress)
                        {
                            stream = new GZipStream(stream, CompressionMode.Decompress, false);
                        }
                        var commandSerializer = new CommandBinarySerializer(stream, serializer);
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

        #region ICommandRepositoryPersist

        /// <inheritdoc />
        void ICommandRepositoryPersist.Save(IDictionary<string, object> dict)
        {
            dict[nameof(logsPath)] = logsPath;
            dict[nameof(buffer)] = buffer;
            dict[nameof(compress)] = compress;
            dict[nameof(serializer)] = serializer.GetType().AssemblyQualifiedName;
            dict[nameof(prefix)] = prefix;
        }

        #endregion

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        /// <returns>Command repository.</returns>
        public static ICommandRepository Create(IDictionary<string, object> dict)
        {
            return new FileCommandRepository(
                dict[nameof(logsPath)].ToString(),
                (IObjectSerializer)Activator.CreateInstance(Type.GetType(dict[nameof(serializer)].ToString())),
                dict[nameof(prefix)].ToString(),
                Convert.ToBoolean(dict[nameof(buffer)]),
                Convert.ToBoolean(dict[nameof(compress)])
            );
        }
    }
}
