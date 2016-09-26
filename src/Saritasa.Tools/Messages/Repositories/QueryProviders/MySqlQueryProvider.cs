// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Repositories.QueryProviders
{
    using System;
    using System.Text;

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
                    INDEX `type` (`type`),
                    INDEX `content_id` (`content_id`),
                    INDEX `content_type` (`content_type`),
                    INDEX `error_type` (`error_type`),
                    INDEX `created_at` (`created_at`)
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
            // TODO
            var sb = new StringBuilder();
            sb.Append($"SELECT * FROM `{TableName}`");
            return sb.ToString();
        }
    }
}
