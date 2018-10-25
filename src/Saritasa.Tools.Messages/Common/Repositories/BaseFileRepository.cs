// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Internal;
using Saritasa.Tools.Messages.Common.ObjectSerializers;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// Base file repository for file, csv, json formats.
    /// </summary>
    public abstract class BaseFileRepository : IMessageRepository
    {
        private const string KeyPath = "path";
        private const string KeyPrefix = "prefix";
        private const string KeyBuffer = "buffer";
        private const string KeySerializer = "serializer";

        /// <summary>
        /// Date format used for file name.
        /// </summary>
        public const string DateTimeFormat = "yyyyMMdd";

        /// <summary>
        /// Current file name prefix.
        /// </summary>
        public string FileNamePrefix { get; protected set; }

        /// <summary>
        /// Current file name extension.
        /// </summary>
        public abstract string FileNameExtension { get; }

        /// <summary>
        /// Serializer used.
        /// </summary>
        public IObjectSerializer Serializer { get; protected set; }

        /// <summary>
        /// Logs path.
        /// </summary>
        public string Path { get; protected set; }

        /// <summary>
        /// Should file stream be buffered. <c>false</c> by default.
        /// </summary>
        public bool BufferStream { get; protected set; }

        /// <summary>
        /// Gets the object which can be used to synchronize asynchronous operations that must rely on the.
        /// </summary>
        protected object SyncRoot { get; } = new object();

        /// <summary>
        /// .ctor
        /// </summary>
        protected BaseFileRepository()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="parameters">Dictionary parameters.</param>
        protected BaseFileRepository(IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            Path = parameters.GetValueOrInvoke(KeyPath, RepositoryConfigurationException.ThrowParameterNotExists);
            FileNamePrefix = parameters.GetValueOrDefault(KeyPrefix, string.Empty);
            BufferStream = Convert.ToBoolean(parameters.GetValueOrDefault(KeyBuffer, false.ToString()));
            if (parameters.ContainsKey(KeySerializer))
            {
                Serializer = (IObjectSerializer)Activator.CreateInstance(
                    Type.GetType(parameters[KeySerializer]));
            }
        }

        /// <summary>
        /// Validate repository and init if needed.
        /// </summary>
        protected virtual void ValidateAndInit()
        {
            if (string.IsNullOrEmpty(Path))
            {
                throw new RepositoryConfigurationException("Path should not be empty.");
            }
            Directory.CreateDirectory(Path);
            if (Serializer == null)
            {
                Serializer = new JsonObjectSerializer();
            }
        }

        #region IMessageRepository

        /// <summary>
        /// Save repository state to dictionary.
        /// </summary>
        /// <param name="parameters">Parameters dicitonary.</param>
        public virtual void SaveState(IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            parameters[KeyPath] = Path;
            parameters[KeyPrefix] = FileNamePrefix;
            parameters[KeyBuffer] = BufferStream.ToString();
            parameters[KeySerializer] = Serializer.GetType().AssemblyQualifiedName;
        }

        /// <inheritdoc />
        public abstract Task AddAsync(MessageRecord messageRecord, CancellationToken cancellationToken = default(CancellationToken));

        /// <inheritdoc />
        public Task<IEnumerable<MessageRecord>> GetAsync(MessageQuery messageQuery,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Collect all files in dir.
            var allFiles =
                Directory.GetFiles(Path, GetSearchPattern()).OrderBy(f => f).Select(System.IO.Path.GetFileName).ToArray();
            var allFilesHash = new HashSet<string>(allFiles);
            if (!allFiles.Any())
            {
                return Task.FromResult(new List<MessageRecord>().AsEnumerable());
            }

            // Init first and last dates.
            var startDate = messageQuery.CreatedStartDate ?? DateTime.MinValue;
            var endDate = messageQuery.CreatedEndDate ?? DateTime.MaxValue.AddDays(-1);

            // Correct start and end dates, so minimum date will be first file in list, and max date last file.
            if (allFiles.Any())
            {
                if (DateTime.TryParseExact(GetFileDatePart(allFiles.First()), DateTimeFormat,
                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out DateTime tmp) && tmp > startDate)
                {
                    startDate = tmp;
                }
                if (DateTime.TryParseExact(GetFileDatePart(allFiles.Last()), DateTimeFormat,
                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.None, out tmp))
                {
                    tmp = tmp.AddDays(1);
                    if (messageQuery.CreatedEndDate.HasValue && tmp < messageQuery.CreatedEndDate.Value)
                    {
                        endDate = tmp;
                    }
                    if (!messageQuery.CreatedEndDate.HasValue)
                    {
                        endDate = tmp;
                    }
                }
            }

            // Actual search.
            var targetList = new List<MessageRecord>(150);
            var date = startDate;
            int skipCount = 0, takeCount = 0;
            while (date <= endDate)
            {
                // Do not go further if we reach take count.
                if (messageQuery.Take > 0 && takeCount >= messageQuery.Take)
                {
                    break;
                }
                for (var i = 0; i < 1000; i++)
                {
                    var fileName = GetFileNameByDate(date, i);
                    if (!allFilesHash.Contains(fileName))
                    {
                        break;
                    }

                    Stream stream = null;
                    try
                    {
                        stream = new FileStream(System.IO.Path.Combine(Path, fileName), FileMode.Open, FileAccess.Read,
                            FileShare.ReadWrite);
                        var messages = ReadMessagesFromStream(stream, messageQuery);
                        foreach (MessageRecord messageRecord in messages)
                        {
                            if (!messageQuery.Match(messageRecord))
                            {
                                continue;
                            }
                            if (messageQuery.Skip > 0 && skipCount < messageQuery.Skip)
                            {
                                skipCount++;
                                continue;
                            }
                            if (messageQuery.Take > 0 && takeCount >= messageQuery.Take)
                            {
                                break;
                            }
                            targetList.Add(messageRecord);
                            takeCount++;
                        }
                    }
                    finally
                    {
                        stream?.Dispose();
                    }
                    cancellationToken.ThrowIfCancellationRequested();

                    if (messageQuery.Take > 0 && takeCount >= messageQuery.Take)
                    {
                        break;
                    }
                }
                date = date.AddDays(1);
            }
            return Task.FromResult(targetList.AsEnumerable());
        }

        #endregion

        /// <summary>
        /// Read messages from stream.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <param name="query">Message query.</param>
        /// <returns>Enumerable of message records.</returns>
        protected abstract IEnumerable<MessageRecord> ReadMessagesFromStream(
            Stream stream,
            MessageQuery query);

        private string GetSearchPattern()
        {
            return "*" + FileNameExtension;
        }

        private string GetFileDatePart(string fileName)
        {
            if (string.IsNullOrEmpty(FileNamePrefix))
            {
                return fileName.Length > 7 ? fileName.Substring(0, 8) : string.Empty;
            }
            else
            {
                var ind = fileName.IndexOf('-');
                return fileName.Substring(ind + 1, 8);
            }
        }

        /// <summary>
        /// Get file name. For example tmp-20170712-000.json .
        /// </summary>
        /// <param name="date">Date.</param>
        /// <param name="count"></param>
        /// <returns></returns>
        protected string GetFileNameByDate(DateTime date, int count)
        {
            var name = $"{date:yyyyMMdd}-{count:000}{FileNameExtension}";
            if (!string.IsNullOrEmpty(FileNamePrefix))
            {
                name = FileNamePrefix + "-" + name;
            }
            return name;
        }

        DateTime currentDate = DateTime.MinValue;

        /// <summary>
        /// Get file name that is available now and relates to date.
        /// </summary>
        /// <param name="currentFileStream">Current file stream.
        /// The method would try to get name from it first.</param>
        /// <param name="date">Date for file.</param>
        /// <param name="useCounter">If counter should be used the new file will be
        /// created for every new stream.</param>
        /// <returns>File name.</returns>
        protected string GetAvailableFileNameByDate(
            FileStream currentFileStream,
            DateTime date,
            bool useCounter = false)
        {
            if (date.Date == currentDate && currentFileStream != null)
            {
                return System.IO.Path.GetFileName(currentFileStream.Name);
            }

            currentDate = date.Date;
            string name = string.Empty;
            for (int i = 0; i < 1000; i++)
            {
                name = GetFileNameByDate(date, i);

                if (useCounter && currentFileStream == null && File.Exists(System.IO.Path.Combine(Path, name)))
                {
                    continue;
                }
                break;
            }
            return name;
        }
    }
}
