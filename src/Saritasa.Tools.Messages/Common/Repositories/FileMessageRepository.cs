// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Abstractions;
    using Internal;
    using ObjectSerializers;

    /// <summary>
    /// Stores to file message.
    /// </summary>
    public class FileMessageRepository : IMessageRepository
    {
        const string DateTimeFormat = "yyyyMMdd";

        /// <summary>
        /// Logs path.
        /// </summary>
        public string LogsPath => logsPath;

        bool disposed;

        FileStream currentFileStream;

        GZipStream currentGZipStream;

        MessageBinarySerializer currentBinarySerializer;

        readonly string logsPath = string.Empty;

        readonly IObjectSerializer serializer;

        readonly string prefix;

        readonly bool buffer;

        readonly bool compress;

        static readonly object objLock = new object();

        Stream CurrentStream => currentGZipStream ?? (Stream)currentFileStream;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="logsPath">Logs path.</param>
        /// <param name="serializer">Object serializer. By default json serializer is used.</param>
        /// <param name="buffer">Should the output stream be buffered.</param>
        /// <param name="compress">Compress target files.</param>
        /// <param name="prefix">Files names prefix.</param>
        public FileMessageRepository(string logsPath, IObjectSerializer serializer = null, string prefix = "",
            bool buffer = true, bool compress = false)
        {
            if (string.IsNullOrEmpty(logsPath))
            {
                throw new ArgumentException(nameof(logsPath));
            }
            this.logsPath = logsPath;
            this.serializer = serializer ?? new JsonObjectSerializer();
            this.prefix = prefix;
            this.buffer = buffer;
            this.compress = compress;
            Directory.CreateDirectory(LogsPath);
        }

        string GetFileNameByDate(DateTime date, int count)
        {
            var name = $"{date:yyyyMMdd}-{count:000}.bin";
            if (!string.IsNullOrEmpty(prefix))
            {
                name = prefix + "-" + name;
            }
            if (compress)
            {
                name += ".zip";
            }
            return name;
        }

        string GetAvailableFileNameByDate(DateTime date)
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
                if (!compress || !File.Exists(Path.Combine(LogsPath, name)))
                {
                    break;
                }
            }
            return name;
        }

        #region IMessageRepository

        static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public Task AddAsync(IMessage context)
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
                    if (compress)
                    {
                        currentGZipStream = new GZipStream(currentFileStream, CompressionMode.Compress);
                    }
                    currentBinarySerializer = new MessageBinarySerializer(CurrentStream, serializer, null);
                }
                currentBinarySerializer.Write(context);
            }

            if (!buffer)
            {
                lock (objLock)
                {
                    currentGZipStream?.Flush();
                    currentFileStream.FlushAsync();
                }
            }

            return completedTask;
        }

        string GetSearchPattern()
        {
            return compress ? prefix + "*.bin.zip" : prefix + "*.bin";
        }

        string GetFileDatePart(string fileName)
        {
            return fileName.Length > 7 ? fileName.Substring(0, 8) : string.Empty;
        }

        /// <inheritdoc />
        public Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery)
        {
            // collect all files in dir
            var allFiles =
                Directory.GetFiles(LogsPath, GetSearchPattern()).OrderBy(f => f).Select(Path.GetFileName).ToArray();
            var allFilesHash = new HashSet<string>(allFiles);

            // init first and last dates
            var startDate = messageQuery.CreatedStartDate ?? DateTime.MinValue;
            var endDate = messageQuery.CreatedEndDate ?? DateTime.MaxValue;

            // correct start and end dates, so minimum date will be first file in list, and max date last file
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
                    if (tmp < messageQuery.CreatedEndDate.Value)
                    {
                        endDate = tmp;
                    }
                }
            }

            // actual search
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
                        stream = new FileStream(Path.Combine(LogsPath, fileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        if (compress)
                        {
                            stream = new GZipStream(stream, CompressionMode.Decompress, false);
                        }
                        var commandSerializer = new MessageBinarySerializer(stream, serializer, messageQuery.Assemblies.ToArray());
                        for (Message message; (message = commandSerializer.Read()) != null;)
                        {
                            if (messageQuery.Match(message))
                            {
                                targetList.Add(message);
                            }
                        }
                    }
                    finally
                    {
                        stream?.Dispose();
                    }
                }
                currentDate = currentDate.AddDays(1);
            }
            return Task.FromResult(targetList.Cast<IMessage>());
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, object> dict)
        {
            dict[nameof(logsPath)] = logsPath;
            dict[nameof(buffer)] = buffer;
            dict[nameof(compress)] = compress;
            dict[nameof(serializer)] = serializer.GetType().AssemblyQualifiedName;
            dict[nameof(prefix)] = prefix;
        }

        #endregion

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
            return new FileMessageRepository(
                dict[nameof(logsPath)].ToString(),
                (IObjectSerializer)Activator.CreateInstance(Type.GetType(dict[nameof(serializer)].ToString())),
                dict[nameof(prefix)].ToString(),
                Convert.ToBoolean(dict[nameof(buffer)]),
                Convert.ToBoolean(dict[nameof(compress)])
            );
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
