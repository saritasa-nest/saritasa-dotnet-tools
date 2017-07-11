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
    /// Csv file target.
    /// </summary>
    public class CsvFileMessageRepository : IMessageRepository, IDisposable
    {
        const string CsvHeader = "Id,Type,CreatedAt,Status,ContentType,Content,Data,ErrorType,ErrorMessage,ErrorDetails,ExecutionDuration"; // 11

        /// <summary>
        /// Logs path.
        /// </summary>
        public string LogsPath { get; }

        bool needWriteHeader;

        FileStream currentFileStream;

        readonly IObjectSerializer serializer;

        readonly string prefix;

        readonly bool buffer;

        static readonly object objLock = new object();

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="logsPath">Logs path.</param>
        /// <param name="serializer">Object serializer. By default json serializer is used.</param>
        /// <param name="buffer">Should the output stream be buffered.</param>
        /// <param name="prefix">Files names prefix.</param>
        public CsvFileMessageRepository(
            string logsPath,
            IObjectSerializer serializer = null,
            string prefix = "",
            bool buffer = true)
        {
            if (string.IsNullOrEmpty(logsPath))
            {
                throw new ArgumentException(nameof(logsPath));
            }
            this.LogsPath = logsPath;
            this.serializer = serializer ?? new JsonObjectSerializer();
            this.prefix = prefix;
            this.buffer = buffer;
            Directory.CreateDirectory(LogsPath);

            if (!this.serializer.IsText)
            {
                throw new ArgumentException(Properties.Strings.SerializerShouldBeText);
            }
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        public CsvFileMessageRepository(IDictionary<string, string> dict)
        {
            this.LogsPath = dict[nameof(LogsPath)].ToString();
            this.serializer = (IObjectSerializer)Activator.CreateInstance(Type.GetType(dict[nameof(serializer)]));
            this.prefix = dict[nameof(prefix)].ToString();
            this.buffer = Convert.ToBoolean(dict[nameof(buffer)]);
        }

        string GetFileNameByDate(DateTime date, int count)
        {
            var name = $"{date:yyyyMMdd}-{count:000}.csv";
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

        static readonly byte[] comma = Encoding.UTF8.GetBytes(",");
        static readonly byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

        private static string PrepareString(string str)
        {
            bool mustQuote = str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n");
            if (mustQuote)
            {
                var sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                    {
                        sb.Append("\"");
                    }
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }

        static void WriteBytes(string str, Stream stream, bool prepareString = true, bool last = false)
        {
            var bytes = Encoding.UTF8.GetBytes(prepareString ? PrepareString(str) : str);
            stream.Write(bytes, 0, bytes.Length);
            if (!last)
            {
                stream.Write(comma, 0, comma.Length);
            }
            else
            {
                stream.Write(newLine, 0, newLine.Length);
            }
        }

        private static void WriteBytes(byte bt, Stream stream)
        {
            stream.WriteByte(bt);
        }

        private void WriteToFile(IMessage message, CancellationToken cancellationToken)
        {
            // Id,Type,CreatedAt,Status,ContentType,Content,Data,ErrorType,ErrorMessage,ErrorDetails,ExecutionDuration
            if (needWriteHeader)
            {
                WriteBytes(CsvHeader, currentFileStream, prepareString: false, last: true);
                needWriteHeader = false;
            }

            WriteBytes(message.Id.ToString(), currentFileStream);
            WriteBytes(message.Type, currentFileStream);
            WriteBytes(message.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"), currentFileStream);
            WriteBytes(message.Status.ToString(), currentFileStream);
            WriteBytes(message.ContentType, currentFileStream);
            WriteBytes(Encoding.UTF8.GetString(serializer.Serialize(message.Content)), currentFileStream);
            WriteBytes(Encoding.UTF8.GetString(serializer.Serialize(message.Data)), currentFileStream);
            WriteBytes(message.ErrorType, currentFileStream);
            WriteBytes(message.ErrorMessage, currentFileStream);
            WriteBytes(Encoding.UTF8.GetString(serializer.Serialize(message.Error)), currentFileStream);
            WriteBytes(message.ExecutionDuration.ToString(), currentFileStream, last: true);
        }

        #region IMessageRepository

        static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public Task AddAsync(IMessage message, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            lock (objLock)
            {
                string name = GetAvailableFileNameByDate(DateTime.Now);
                if (currentFileStream == null || Path.GetFileName(currentFileStream.Name) != name)
                {
                    Close();
                    currentFileStream = new FileStream(Path.Combine(LogsPath, name), FileMode.Append);
                    needWriteHeader = currentFileStream.Length == 0;
                }
                WriteToFile(message, cancellationToken);
            }

            if (!buffer)
            {
                lock (objLock)
                {
                    currentFileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            return completedTask;
        }

        /// <inheritdoc />
        public Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, string> dict)
        {
            dict[nameof(LogsPath)] = LogsPath;
            dict[nameof(buffer)] = buffer.ToString();
            dict[nameof(serializer)] = serializer.GetType().AssemblyQualifiedName;
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
