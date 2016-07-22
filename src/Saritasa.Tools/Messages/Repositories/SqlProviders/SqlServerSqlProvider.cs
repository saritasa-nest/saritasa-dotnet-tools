// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Repositories.SqlProviders
{
    using System;

    /// <summary>
    /// SQL Server sql scripts.
    /// </summary>
    internal class SqlServerSqlProvider : ISqlProvider
    {
        const string TableName = "SaritasaMessages";

        IObjectSerializer serializer;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="serializer">Used object serializer.</param>
        public SqlServerSqlProvider(IObjectSerializer serializer)
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
                CREATE TABLE [{TableName}] (
                    Id bigint IDENTITY,
                    Type tinyint NOT NULL,
                    ContentId char(32) NOT NULL,
                    ContentType varchar(255) NOT NULL,
                    Content {(serializer.IsText ? "nvarchar(max)" : "varbinary(max)")} NOT NULL,
                    Data {(serializer.IsText ? "nvarchar(max)" : "varbinary(max)")},
                    ErrorDetails {(serializer.IsText ? "nvarchar(max)" : "varbinary(max)")},
                    ErrorMessage varchar(255) NOT NULL DEFAULT '',
                    ErrorType varchar(255) NOT NULL DEFAULT '',
                    CreatedAt datetime NOT NULL,
                    ExecutionDuration int NOT NULL,
                    Status tinyint NOT NULL,

                    CONSTRAINT PK_{TableName} PRIMARY KEY CLUSTERED (Id),
                    INDEX IX_{TableName}_Type (Type),
                    INDEX IX_{TableName}_ContentId (ContentId),
                    INDEX IX_{TableName}_ContentType (ContentType),
                    INDEX IX_{TableName}_ErrorType (ErrorType),
                    INDEX IX_{TableName}_CreatedAt (CreatedAt)
                );";
        }

        /// <inheritdoc />
        public string GetExistsTableScript()
        {
            return $"SELECT * FROM sys.objects WHERE object_id = object_id('{TableName}') AND type IN ('U');";
        }

        /// <inheritdoc />
        public string GetInsertMessageScript()
        {
            return $@"
                INSERT INTO [{TableName}] VALUES
                (@Type, @ContentId, @ContentType, @Content, @Data, @ErrorDetails, @ErrorMessage, @ErrorType, @CreatedAt, @ExecutionDuration, @Status);
            ";
        }
    }
}
