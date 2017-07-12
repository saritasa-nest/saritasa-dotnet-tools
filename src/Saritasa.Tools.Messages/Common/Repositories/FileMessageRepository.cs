// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        private string GetSearchPattern()
        {
            return "*" + FileNameExtension;
        }

        private string GetFileDatePart(string fileName)
        {
            return fileName.Length > 7 ? fileName.Substring(0, 8) : string.Empty;
        }

        /// <inheritdoc />
        public Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery, CancellationToken cancellationToken)
        {
            // Collect all files in dir.
            var allFiles =
                Directory.GetFiles(Path, GetSearchPattern()).OrderBy(f => f).Select(System.IO.Path.GetFileName).ToArray();
            var allFilesHash = new HashSet<string>(allFiles);

            // Init first and last dates.
            var startDate = messageQuery.CreatedStartDate ?? DateTime.MinValue;
            var endDate = messageQuery.CreatedEndDate ?? DateTime.MaxValue;

            // Correct start and end dates, so minimum date will be first file in list, and max date last file.
            if (allFiles.Any())
            {
                DateTime tmp;
                if (DateTime.TryParseExact(GetFileDatePart(allFiles.First()), DateTimeFormat,
                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out tmp) && tmp > startDate)
                {
                    startDate = tmp;
                }
                if (DateTime.TryParseExact(GetFileDatePart(allFiles.Last()), DateTimeFormat,
                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out tmp))
                {
                    tmp = tmp.AddDays(1);
                    if (messageQuery.CreatedEndDate != null && tmp < messageQuery.CreatedEndDate.Value)
                    {
                        endDate = tmp;
                    }
                }
            }

            // Actual search.
            var targetList = new List<Message>(150);
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                for (var i = 0; i < 1000; i++)
                {
                    var fileName = GetFileNameByDate(currentDate, i);
                    if (!allFilesHash.Contains(fileName))
                    {
                        break;
                    }

                    Stream stream = null;
                    try
                    {
                        stream = new FileStream(System.IO.Path.Combine(Path, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        if (compress)
                        {
                            stream = new GZipStream(stream, CompressionMode.Decompress, false);
                        }
                        var commandSerializer = new MessageBinarySerializer(stream, Serializer, messageQuery.Assemblies.ToArray());
                        for (Message message; (message = commandSerializer.Read()) != null;)
                        {
                            if (messageQuery.Match(message))
                            {
                                targetList.Add(message);
                            }
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                    finally
                    {
                        stream?.Dispose();
                    }
                }
                currentDate = currentDate.AddDays(1);
                cancellationToken.ThrowIfCancellationRequested();
            }
            return Task.FromResult(targetList.Cast<IMessage>());
        }

        /// <inheritdoc />
        public override void SaveState(IDictionary<string, string> parameters)
        {
            base.SaveState(parameters);
            parameters[KeyCompress] = compress.ToString();
        }

        #endregion

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

    /// <summary>
    /// Search for CreatedAt &gt; X AND CreatedAt &lt; X.
    /// </summary>
    internal class CreatedDateExpressionVisitor : ExpressionVisitor
    {
        public DateTime StartDate { get; set; } = DateTime.MinValue;

        public DateTime EndDate { get; set; } = DateTime.MaxValue;

        DateTime GetDate(BinaryExpression conditionalNode)
        {
            if (conditionalNode.Left.NodeType == ExpressionType.MemberAccess && conditionalNode.Right.NodeType == ExpressionType.MemberAccess)
            {
                var leftMemberNode = (MemberExpression)conditionalNode.Left;
                if (leftMemberNode.Member.Name != nameof(Message.CreatedAt))
                {
                    return DateTime.MinValue;
                }
                var memberNode = (MemberExpression)conditionalNode.Right;
                var constantExpression = (ConstantExpression)memberNode.Expression;
                return ((DateTime)((FieldInfo)memberNode.Member).GetValue(constantExpression.Value)).Date;
            }
            return DateTime.MinValue;
        }

        public override Expression Visit(Expression node)
        {
            if (node.NodeType == ExpressionType.GreaterThan || node.NodeType == ExpressionType.GreaterThanOrEqual)
            {
                var date = GetDate((BinaryExpression)node);
                if (date > StartDate)
                {
                    StartDate = date;
                }
            }
            else if (node.NodeType == ExpressionType.LessThan || node.NodeType == ExpressionType.LessThanOrEqual)
            {
                var date = GetDate((BinaryExpression)node);
                if (date < EndDate)
                {
                    EndDate = date;
                }
            }

            return base.Visit(node);
        }
    }
}
