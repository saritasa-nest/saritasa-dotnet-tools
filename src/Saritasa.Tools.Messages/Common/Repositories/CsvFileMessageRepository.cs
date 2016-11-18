// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common.Repositories
{
    using ObjectSerializers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Csv file target.
    /// </summary>
    public class CsvFileMessageRepository : IMessageRepository, IDisposable
    {
        const string CsvHeader = "Id,Type,CreatedAt,Status,ContentType,Content,Data,ErrorType,ErrorMessage,ErrorDetails,ExecutionDuration"; // 11

        /// <summary>
        /// Logs path.
        /// </summary>
        public string LogsPath { get; } = string.Empty;

        bool disposed;

        bool needWriteHeader;

        FileStream currentFileStream;

        readonly IObjectSerializer serializer;

        readonly string prefix;

        readonly bool buffer;

        static readonly object ObjLock = new object();

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="logsPath">Logs path.</param>
        /// <param name="serializer">Object serializer. By default json serializer is used.</param>
        /// <param name="buffer">Should the output stream be buffered.</param>
        /// <param name="prefix">Files names prefix.</param>
        public CsvFileMessageRepository(string logsPath, IObjectSerializer serializer = null, string prefix = "",
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
                throw new ArgumentException("Serializer should be text-based");
            }
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

        static readonly byte[] Comma = Encoding.UTF8.GetBytes(",");
        static readonly byte[] NewLine = Encoding.UTF8.GetBytes(Environment.NewLine);

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
                stream.Write(Comma, 0, Comma.Length);
            }
            else
            {
                stream.Write(NewLine, 0, NewLine.Length);
            }
        }

        static void WriteBytes(byte bt, Stream stream)
        {
            stream.WriteByte(bt);
        }

        void WriteToFile(Message message)
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

        /// <inheritdoc />
        public void Add(Message message)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("The repository has been disposed.");
            }

            lock (ObjLock)
            {
                string name = GetAvailableFileNameByDate(DateTime.Now);
                if (currentFileStream == null || Path.GetFileName(currentFileStream.Name) != name)
                {
                    Close();
                    currentFileStream = new FileStream(Path.Combine(LogsPath, name), FileMode.Append);
                    needWriteHeader = currentFileStream.Length == 0;
                }
                WriteToFile(message);
            }

            if (!buffer)
            {
                lock (ObjLock)
                {
                    currentFileStream.FlushAsync();
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<Message> Get(MessageQuery messageQuery)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, object> dict)
        {
            dict[nameof(LogsPath)] = LogsPath;
            dict[nameof(buffer)] = buffer;
            dict[nameof(serializer)] = serializer.GetType().AssemblyQualifiedName;
            dict[nameof(prefix)] = prefix;
        }

        #endregion

        /// <summary>
        /// Close all streams.
        /// </summary>
        public void Close()
        {
            if (currentFileStream != null)
            {
                currentFileStream.Dispose();
                currentFileStream = null;
            }
        }

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

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        /// <returns>Message repository.</returns>
        public static IMessageRepository CreateFromState(IDictionary<string, object> dict)
        {
            return new CsvFileMessageRepository(
                dict[nameof(LogsPath)].ToString(),
                (IObjectSerializer)Activator.CreateInstance(Type.GetType(dict[nameof(serializer)].ToString())),
                dict[nameof(prefix)].ToString(),
                Convert.ToBoolean(dict[nameof(buffer)])
            );
        }
    }
}
