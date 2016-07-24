// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Repositories
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
        const string DateTimeFormat = "yyyyMMdd";
        const string CsvHeader = "Id,Type,CreatedAt,Status,ContentType,Content,Data,ErrorType,ErrorMessage,ErrorDetails,ExecutionDuration"; // 11

        /// <summary>
        /// Logs path.
        /// </summary>
        public string LogsPath
        {
            get { return logsPath; }
        }

        bool disposed;

        bool needWriteHeader;

        FileStream currentFileStream;

        readonly string logsPath = string.Empty;

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
            this.logsPath = logsPath;
            this.serializer = serializer != null ? serializer : new JsonObjectSerializer();
            this.prefix = prefix;
            this.buffer = buffer;
            Directory.CreateDirectory(LogsPath);

            if (this.serializer.IsText == false)
            {
                throw new ArgumentException("Serializer should be text-based");
            }
        }

        private string GetFileNameByDate(DateTime date, int count)
        {
            var name = $"{date.ToString(DateTimeFormat)}-{count:000}.csv";
            if (string.IsNullOrEmpty(prefix) == false)
            {
                name = prefix + "-" + name;
            }
            return name;
        }

        private string GetAvailableFileNameByDate(DateTime date)
        {
            if (currentFileStream != null)
            {
                return Path.GetFileName(currentFileStream.Name);
            }

            return GetFileNameByDate(date, 0);
        }

        static byte[] comma = Encoding.UTF8.GetBytes(",");
        static byte[] newLine = Encoding.UTF8.GetBytes(Environment.NewLine);

        private static string PrepareString(string str)
        {
            bool mustQuote = str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n");
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
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

        private static void WriteBytes(string str, Stream stream, bool prepareString = true, bool last = false)
        {
            if (prepareString)
            {
                str = PrepareString(str);
            }
            var bytes = Encoding.UTF8.GetBytes(str);
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

        private void WriteToFile(Message message)
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
            WriteBytes(Encoding.UTF8.GetString(serializer.Serialize(message.ErrorDetails)), currentFileStream);
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
        public IEnumerable<Message> Get(Expression<Func<Message, bool>> selector, Assembly[] assemblies = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, object> dict)
        {
            dict[nameof(logsPath)] = logsPath;
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

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        /// <returns>Message repository.</returns>
        public static IMessageRepository CreateFromState(IDictionary<string, object> dict)
        {
            return new CsvFileMessageRepository(
                dict[nameof(logsPath)].ToString(),
                (IObjectSerializer)Activator.CreateInstance(Type.GetType(dict[nameof(serializer)].ToString())),
                dict[nameof(prefix)].ToString(),
                Convert.ToBoolean(dict[nameof(buffer)])
            );
        }
    }
}
