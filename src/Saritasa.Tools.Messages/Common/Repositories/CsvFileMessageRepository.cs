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
    public class CsvFileMessageRepository : BaseFileRepository, IMessageRepository, IDisposable
    {
        private const string KeyNeedWriteHeader = "writeheader";

        private const string CsvHeader = "Id,Type,CreatedAt,Status,ContentType,Content,Data,ErrorType,ErrorMessage,ErrorDetails,ExecutionDuration"; // 11

        private bool needWriteHeader;

        private FileStream currentFileStream;

        /// <inheritdoc />
        public override string FileNameExtension => ".csv";

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="path">Logs path.</param>
        /// <param name="serializer">Object serializer. By default json serializer is used.</param>
        /// <param name="buffer">Should the output stream be buffered.</param>
        /// <param name="prefix">Files names prefix.</param>
        public CsvFileMessageRepository(
            string path,
            IObjectSerializer serializer = null,
            string prefix = "",
            bool buffer = true)
        {
            this.Path = path;
            this.Serializer = serializer ?? new JsonObjectSerializer();
            this.FileNamePrefix = prefix;
            this.BufferStream = buffer;

            ValidateAndInit();
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        public CsvFileMessageRepository(IDictionary<string, string> parameters)
            : base(parameters)
        {
            ValidateAndInit();
        }

        /// <inheritdoc />
        protected override void ValidateAndInit()
        {
            base.ValidateAndInit();
            if (!this.Serializer.IsText)
            {
                throw new ArgumentException(Properties.Strings.SerializerShouldBeText);
            }
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

        private void WriteToFile(MessageRecord messageRecord, CancellationToken cancellationToken)
        {
            // Id,Type,CreatedAt,Status,ContentType,Content,Data,ErrorType,ErrorMessage,ErrorDetails,ExecutionDuration
            if (needWriteHeader)
            {
                WriteBytes(CsvHeader, currentFileStream, prepareString: false, last: true);
                needWriteHeader = false;
            }

            WriteBytes(messageRecord.Id.ToString(), currentFileStream);
            WriteBytes(messageRecord.Type, currentFileStream);
            WriteBytes(messageRecord.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"), currentFileStream);
            WriteBytes(messageRecord.Status.ToString(), currentFileStream);
            WriteBytes(messageRecord.ContentType, currentFileStream);
            WriteBytes(Encoding.UTF8.GetString(Serializer.Serialize(messageRecord.Content)), currentFileStream);
            WriteBytes(Encoding.UTF8.GetString(Serializer.Serialize(messageRecord.Data)), currentFileStream);
            WriteBytes(messageRecord.ErrorType, currentFileStream);
            WriteBytes(messageRecord.ErrorMessage, currentFileStream);
            WriteBytes(Encoding.UTF8.GetString(Serializer.Serialize(messageRecord.Error)), currentFileStream);
            WriteBytes(messageRecord.ExecutionDuration.ToString(), currentFileStream, last: true);
        }

        #region IMessageRepository

        static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public Task AddAsync(MessageRecord message, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(null);
            }

            lock (SyncRoot)
            {
                string name = GetAvailableFileNameByDate(currentFileStream, DateTime.Now);
                if (currentFileStream == null || System.IO.Path.GetFileName(currentFileStream.Name) != name)
                {
                    Close();
                    currentFileStream = new FileStream(System.IO.Path.Combine(Path, name), FileMode.Append);
                    needWriteHeader = currentFileStream.Length == 0;
                }
                WriteToFile(message, cancellationToken);
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
        protected void Dispose(bool disposing)
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
