// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

//[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Saritasa.Tools.Messages.Tests")]

using System.Collections.Generic;
using Saritasa.Tools.Messages.Internal;
using Saritasa.Tools.Messages.Internal.Enums;

namespace Saritasa.Tools.Messages.Common.Repositories.QueryProviders
{
    using System;
    using System.Text;

    /// <summary>
    /// SQL Server sql scripts.
    /// </summary>
    public class SqlServerQueryProvider : IMessageQueryProvider
    {
        const string TableName = "SaritasaMessages";

        readonly IObjectSerializer serializer;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="serializer">Used object serializer.</param>
        public SqlServerQueryProvider(IObjectSerializer serializer)
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
                    ContentId uniqueidentifier NOT NULL,
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

        static readonly string[] FieldsList =
        {
            "ContentId",
            "Type",
            "ContentType",
            "Content",
            "Data",
            "ErrorDetails",
            "ErrorMessage",
            "ErrorType",
            "CreatedAt",
            "ExecutionDuration",
            "Status",
        };

        /// <inheritdoc />
        public string GetFilterScript(MessageQuery messageQuery)
        {
            if (messageQuery == null)
            {
                throw new ArgumentNullException(nameof(messageQuery));
            }
            
            return BuildSelectString(messageQuery, new SqlServerSelectStringBuilder());
        }

        private static string BuildSelectString(MessageQuery messageQuery, ISelectStringBuilder ssb)
        {
            ssb.SelectAll().From(TableName);

            if (messageQuery.Id != null)
            {
                ssb.Where("ContentId").EqualsTo(messageQuery.Id);
            }
            if (messageQuery.CreatedStartDate != null)
            {
                ssb.Where("CreatedAt").GreaterOrEqualsTo(messageQuery.CreatedStartDate);
            }
            if (messageQuery.CreatedEndDate != null)
            {
                ssb.Where("CreatedAt").LessOrEqualsTo(messageQuery.CreatedEndDate);
            }
            if (messageQuery.ContentType != null)
            {
                ssb.Where("ContentType").EqualsTo(messageQuery.ContentType);
            }
            if (messageQuery.ErrorType != null)
            {
                ssb.Where("ErrorType").EqualsTo(messageQuery.ErrorType);
            }
            if (messageQuery.Status != null)
            {
                ssb.Where("Status").EqualsTo(messageQuery.Status);
            }
            if (messageQuery.Type != null)
            {
                ssb.Where("Type").EqualsTo(messageQuery.Type);
            }
            if (messageQuery.ExecutionDurationAbove != null)
            {
                ssb.Where("ExecutionDuration").GreaterOrEqualsTo(messageQuery.ExecutionDurationAbove);
            }
            if (messageQuery.ExecutionDurationBelow != null)
            {
                ssb.Where("ExecutionDuration").LessOrEqualsTo(messageQuery.ExecutionDurationBelow);
            }
            if (messageQuery.Skip > 0)
            {
                ssb.OrderBy("ContentId").Skip(messageQuery.Skip);
            }
            if (messageQuery.Take > 0)
            {
                ssb.Take(messageQuery.Take);
            }

            return ssb.Build();
        }
    }
}
