// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common.Repositories.QueryProviders
{
    using System;
    using Internal;

    /// <summary>
    /// MySql sql scripts.
    /// </summary>
    internal class MySqlQueryProvider : IMessageQueryProvider
    {
        const string TableName = "saritasa_messages";

        readonly IObjectSerializer serializer;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="serializer">Used object serializer.</param>
        public MySqlQueryProvider(IObjectSerializer serializer)
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
                CREATE TABLE `{TableName}` (
                    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
                    `type` TINYINT NOT NULL,
                    `content_id` CHAR(32) NOT NULL,
                    `content_type` VARCHAR(255) NOT NULL,
                    `content` {(serializer.IsText ? "MEDIUMTEXT" : "MEDIUMBLOB")} NOT NULL,
                    `data` {(serializer.IsText ? "MEDIUMTEXT" : "MEDIUMBLOB")},
                    `error_details` {(serializer.IsText ? "MEDIUMTEXT" : "MEDIUMBLOB")},
                    `error_message` VARCHAR(255) NOT NULL DEFAULT '\'\'',
                    `error_type` VARCHAR(255) NOT NULL DEFAULT '\'\'',
                    `created_at` DATETIME NOT NULL,
                    `execution_duration` INT NOT NULL,
                    `status` TINYINT NOT NULL,
                    PRIMARY KEY (`id`),
                    INDEX `ix_type` (`type`),
                    INDEX `ix_content_id` (`content_id`),
                    INDEX `ix_content_type` (`content_type`),
                    INDEX `ix_error_type` (`error_type`),
                    INDEX `ix_created_at` (`created_at`)
                )";
        }

        /// <inheritdoc />
        public string GetExistsTableScript()
        {
            return $"SHOW TABLES LIKE '{TableName}'";
        }

        /// <inheritdoc />
        public string GetInsertMessageScript()
        {
            return $@"
                INSERT INTO `{TableName}` VALUES
                (NULL, @Type, @ContentId, @ContentType, @Content, @Data, @ErrorDetails, @ErrorMessage, @ErrorType, @CreatedAt, @ExecutionDuration, @Status);
            ";
        }

        static readonly string[] fieldsList =
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

            return BuildSelectString(messageQuery, new MySqlSelectStringBuilder());
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
