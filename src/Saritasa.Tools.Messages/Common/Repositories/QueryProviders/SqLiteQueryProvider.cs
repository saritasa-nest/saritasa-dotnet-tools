// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common.Repositories.QueryProviders
{
    using System;
    using Internal;

    /// <summary>
    /// SqLite sql scripts.
    /// </summary>
    internal class SqliteQueryProvider : IMessageQueryProvider
    {
        const string TableName = "saritasa_messages";

        readonly IObjectSerializer serializer;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="serializer">Used object serializer.</param>
        public SqliteQueryProvider(IObjectSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            this.serializer = serializer;
        }

        /// <inheritdoc />
        public string GetCreateTableScript()
        {
            return $@"
                CREATE TABLE IF NOT EXISTS {TableName} (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    type TINYINT NOT NULL,
                    content_id CHARACTER(32) NOT NULL,
                    content_type VARYING CHARACTER(255) NOT NULL,
                    content {(serializer.IsText ? "TEXT" : "BLOB")} NOT NULL,
                    data {(serializer.IsText ? "TEXT" : "BLOB")},
                    error_details {(serializer.IsText ? "TEXT" : "BLOB")},
                    error_message VARYING CHARACTER(255) NOT NULL DEFAULT '',
                    error_type VARYING CHARACTER(255) NOT NULL DEFAULT '',
                    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    execution_duration INTEGER NOT NULL,
                    status TINYINT NOT NULL
                );
                CREATE INDEX ix_type ON {TableName} (type);
                CREATE INDEX ix_content_id ON {TableName} (content_id);
                CREATE INDEX ix_content_type ON {TableName} (content_type);
                CREATE INDEX ix_error_type ON {TableName} (error_type);
                CREATE INDEX ix_created_at ON {TableName} (created_at);
";
        }

        /// <inheritdoc />
        public string GetExistsTableScript()
        {
            return $"SELECT name FROM sqlite_master WHERE type='table' AND name= '{TableName}'";
        }

        /// <inheritdoc />
        public string GetInsertMessageScript()
        {
            return $@"
                INSERT INTO {TableName} VALUES
                (NULL, @Type, @ContentId, @ContentType, @Content, @Data, @ErrorDetails, @ErrorMessage, @ErrorType, @CreatedAt, @ExecutionDuration, @Status);
            ";
        }

        static readonly string[] FieldsList =
        {
            "content_id",
            "type",
            "content_type",
            "content",
            "data",
            "error_details",
            "error_message",
            "error_type",
            "created_at",
            "execution_duration",
            "status",
        };

        /// <inheritdoc />
        public string GetFilterScript(MessageQuery messageQuery)
        {
            if (messageQuery == null)
            {
                throw new ArgumentNullException(nameof(messageQuery));
            }

            return BuildSelectString(messageQuery, new SqLiteSelectStringBuilder());
        }

        private static string BuildSelectString(MessageQuery messageQuery, ISelectStringBuilder ssb)
        {
            ssb.SelectAll().From(TableName);

            if (messageQuery.Id != null)
            {
                ssb.Where("content_id").EqualsTo(messageQuery.Id);
            }
            if (messageQuery.CreatedStartDate != null)
            {
                ssb.Where("created_at").GreaterOrEqualsTo(messageQuery.CreatedStartDate);
            }
            if (messageQuery.CreatedEndDate != null)
            {
                ssb.Where("created_at").LessOrEqualsTo(messageQuery.CreatedEndDate);
            }
            if (messageQuery.ContentType != null)
            {
                ssb.Where("content_type").EqualsTo(messageQuery.ContentType);
            }
            if (messageQuery.ErrorType != null)
            {
                ssb.Where("error_type").EqualsTo(messageQuery.ErrorType);
            }
            if (messageQuery.Status != null)
            {
                ssb.Where("status").EqualsTo(messageQuery.Status);
            }
            if (messageQuery.Type != null)
            {
                ssb.Where("type").EqualsTo(messageQuery.Type);
            }
            if (messageQuery.ExecutionDurationAbove != null)
            {
                ssb.Where("execution_duration").GreaterOrEqualsTo(messageQuery.ExecutionDurationAbove);
            }
            if (messageQuery.ExecutionDurationBelow != null)
            {
                ssb.Where("execution_duration").LessOrEqualsTo(messageQuery.ExecutionDurationBelow);
            }
            if (messageQuery.Skip > 0)
            {
                ssb.Skip(messageQuery.Skip);
            }
            if (messageQuery.Take > 0)
            {
                ssb.Take(messageQuery.Take);
            }

            return ssb.Build();
        }
    }
}
