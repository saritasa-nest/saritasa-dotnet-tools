// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Common.ObjectSerializers;
using Saritasa.Tools.Messages.Common.Repositories.QueryProviders;
using Saritasa.Tools.Messages.Internal;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// Use ADO.NET infrastructure to store messages.
    /// </summary>
    public class AdoNetMessageRepository : IMessageRepository, IDisposable
    {
        private const string KeyDialect = "dialect";
        private const string KeyKeepConnection = "keepconnection";
        private const string KeyFactory = "factory";
        private const string KeyConnectionString = "connectionstring";
        private const string KeySerializer = "serializer";

        /// <summary>
        /// Possible SQL dialects.
        /// </summary>
        public enum Dialect
        {
            /// <summary>
            /// Try to determine dialect type from factory.
            /// </summary>
            Auto,

            /// <summary>
            /// MySQL.
            /// </summary>
            MySql,

            /// <summary>
            /// Microsoft SQL Server.
            /// </summary>
            SqlServer,

            /// <summary>
            /// PostgreSQL.
            /// </summary>
            Postgres,

            /// <summary>
            /// SQLite.
            /// </summary>
            Sqlite,

            /// <summary>
            /// Oracle.
            /// </summary>
            Oracle,
        }

        private readonly Dialect dialect;

        private bool isInitialized;

        private DbConnection activeConnection;

        private readonly DbProviderFactory factory;

        private readonly string connectionString;

        private IObjectSerializer serializer;

        private IMessageQueryProvider queryProvider;

        private readonly object objLock = new object();

        /// <summary>
        /// Keep connection opened between queries. False by default.
        /// </summary>
        public bool KeepConnection { get; set; }

        /// <summary>
        /// ADO.NET repository.
        /// </summary>
        /// <param name="factory">Database factory class to be used.</param>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="dialect">Sql dialect to be used.</param>
        /// <param name="serializer">Object serializer for messages.</param>
        public AdoNetMessageRepository(DbProviderFactory factory, string connectionString, Dialect dialect = Dialect.Auto,
            IObjectSerializer serializer = null)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            this.dialect = dialect;
            this.connectionString = connectionString;
            this.factory = factory;
            this.serializer = serializer;
            ValidateAndInit();
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="parameters">Parameters dictionary.</param>
        public AdoNetMessageRepository(IDictionary<string, string> parameters)
        {
#if NETSTANDARD1_5
            throw new NotSupportedException("Not sure how to handle DbProviderFactories for .NET Core.");
#else
            this.factory = DbProviderFactories.GetFactory(
                parameters.GetValueOrInvoke(KeyFactory, RepositoryConfigurationException.ThrowParameterNotExists));
            this.KeepConnection = Convert.ToBoolean(
                parameters.GetValueOrDefault(KeyKeepConnection, true.ToString()));
            this.connectionString = parameters.GetValueOrInvoke(KeyConnectionString,
                RepositoryConfigurationException.ThrowParameterNotExists);
            this.dialect = (Dialect)Enum.Parse(typeof(Dialect), parameters.GetValueOrDefault(KeyDialect, Dialect.Auto.ToString()));
            if (parameters.ContainsKey(KeySerializer))
            {
                this.serializer = (IObjectSerializer)Activator.CreateInstance(Type.GetType(parameters[KeySerializer]));
            }
            ValidateAndInit();
#endif
        }

        private void ValidateAndInit()
        {
            this.serializer = serializer ?? new JsonObjectSerializer();
            this.queryProvider = CreateSqlProvider(this.dialect, this.factory, this.serializer);
        }

        private static IMessageQueryProvider CreateSqlProvider(Dialect dialect, DbProviderFactory factory, IObjectSerializer serializer)
        {
            switch (dialect)
            {
                case Dialect.Auto:
                    var provider = CreateSqlProviderByFactory(factory, serializer);
                    if (provider == null)
                    {
                        throw new NotImplementedException($"The sql provider for factory {factory} is not implemented yet.");
                    }
                    return provider;
                case Dialect.MySql:
                    return new MySqlQueryProvider(serializer);
                case Dialect.SqlServer:
                    return new SqlServerQueryProvider(serializer);
                case Dialect.Sqlite:
                    return new SqliteQueryProvider(serializer);
                default:
                    throw new NotImplementedException($"The sql provider {dialect} is not implemented yet.");
            }
        }

        static IMessageQueryProvider CreateSqlProviderByFactory(DbProviderFactory factory, IObjectSerializer serializer)
        {
            var name = factory.GetType().Name.ToLowerInvariant();
            if (name.Contains("mysql"))
            {
                return new MySqlQueryProvider(serializer);
            }
            if (name.Contains("sqlite"))
            {
                return new SqliteQueryProvider(serializer);
            }
            if (name.Contains("sqlclient"))
            {
                return new SqlServerQueryProvider(serializer);
            }
            return null;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "Parameters are used")]
        public async Task AddAsync(MessageRecord messageRecord, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AdoNetMessageRepository));
            }
            if (!isInitialized)
            {
                lock (objLock)
                {
                    Init(cancellationToken);
                }
                isInitialized = true;
            }

            /*if (KeepConnection)
            {
                DbConnection connection = GetConnection();
                await ExecuteAddMessageCommandAsync(connection, message, cancellationToken);
            }
            else
            {
                using (var connection = GetConnection())
                {
                    await ExecuteAddMessageCommandAsync(connection, message, cancellationToken);
                    connection.Close();
                }
            }*/

            DbConnection connection = null;
            try
            {
                connection = GetConnection();
                await ExecuteAddMessageCommandAsync(connection, messageRecord, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (!KeepConnection && connection != null)
                {
                    connection.Close();
                    connection = null;
                }
            }
        }

        private async Task ExecuteAddMessageCommandAsync(DbConnection connection, MessageRecord messageRecord,
            CancellationToken cancellationToken)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = queryProvider.GetInsertMessageScript();
                AddParameter(command, "@Type", messageRecord.Type);
                AddParameter(command, "@ContentId", messageRecord.Id.ToString());
                AddParameter(command, "@ContentType", messageRecord.ContentType);
                AddParameter(command, "@Content", serializer.Serialize(messageRecord.Content));
                AddParameter(command, "@Data", messageRecord.Data != null ? serializer.Serialize(messageRecord.Data) : null);
                AddParameter(command, "@ErrorDetails", messageRecord.Error != null ? serializer.Serialize(messageRecord.Error) : null);
                AddParameter(command, "@ErrorMessage", messageRecord.ErrorMessage);
                AddParameter(command, "@ErrorType", messageRecord.ErrorType);
                AddParameter(command, "@CreatedAt", messageRecord.CreatedAt);
                AddParameter(command, "@ExecutionDuration", messageRecord.ExecutionDuration);
                AddParameter(command, "@Status", (byte)messageRecord.Status);
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private void AddParameter(IDbCommand cmd, string name, object value)
        {
            var param = factory.CreateParameter();
            if (param != null)
            {
                param.ParameterName = name;
                var locvalue = value;
                if (locvalue is byte[] && serializer.IsText)
                {
                    locvalue = Encoding.UTF8.GetString((byte[])locvalue);
                }
                param.Value = locvalue ?? DBNull.Value;
                param.Direction = ParameterDirection.Input;
                cmd.Parameters.Add(param);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "Parameters are used")]
        private void Init(CancellationToken cancellationToken)
        {
            DbConnection connection = null;
            try
            {
                connection = GetConnection();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = queryProvider.GetExistsTableScript();
                    var messageTable = command.ExecuteScalar();
                    cancellationToken.ThrowIfCancellationRequested();
                    if (messageTable == null)
                    {
                        command.CommandText = queryProvider.GetCreateTableScript();
                        command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                if (!KeepConnection && connection != null)
                {
                    connection.Close();
                    connection = null;
                }
            }
        }

        private DbConnection GetConnection()
        {
            lock (objLock)
            {
                if (KeepConnection)
                {
                    if (activeConnection == null || activeConnection.State != ConnectionState.Open)
                    {
                        activeConnection = factory.CreateConnection();
                        if (activeConnection != null)
                        {
                            activeConnection.ConnectionString = connectionString;
                            activeConnection.Open();
                        }
                    }
                    return activeConnection;
                }
                else
                {
                    var connection = factory.CreateConnection();
                    if (connection != null)
                    {
                        connection.ConnectionString = connectionString;
                        connection.Open();
                    }
                    return connection;
                }
            }
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "Parameters are used")]
        public async Task<IEnumerable<MessageRecord>> GetAsync(MessageQuery messageQuery,
            CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AdoNetMessageRepository));
            }

            // Execute.
            var messages = new List<MessageRecord>();
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = queryProvider.GetFilterScript(messageQuery);
                using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var messageRecord = new MessageRecord
                        {
                            Type = reader.GetByte(1),
                            Id = reader.GetGuid(2),
                            ContentType = reader.GetString(3)
                        };
                        var content = serializer.IsText ? Encoding.UTF8.GetBytes(reader.GetString(4)) : (byte[])reader[4];
                        TypeHelpers.ResolveTypeForContent(messageRecord, content, serializer, messageQuery.Assemblies.ToArray());
                        messageRecord.Data = (IDictionary<string, string>)serializer.Deserialize(
                            Encoding.UTF8.GetBytes(reader.GetString(5)), typeof(IDictionary<string, string>));
                        var error = serializer.IsText ? Encoding.UTF8.GetBytes(reader.GetString(7)) : (byte[])reader[7];
                        TypeHelpers.ResolveTypeForError(messageRecord, error, serializer, messageQuery.Assemblies.ToArray());
                        messageRecord.ErrorMessage = reader.GetString(7);
                        messageRecord.ErrorType = reader.GetString(8);
                        messageRecord.CreatedAt = reader.GetDateTime(9);
                        messageRecord.ExecutionDuration = reader.GetInt32(10);
                        messageRecord.Status = (ProcessingStatus)reader.GetByte(11);

                        messages.Add(messageRecord);
                    }
                }
            }

            return messages;
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, string> parameters)
        {
            parameters[KeyDialect] = dialect.ToString();
            parameters[KeyKeepConnection] = KeepConnection.ToString();
            parameters[KeyFactory] = factory.GetType().Namespace;
            parameters[KeyConnectionString] = connectionString;
            parameters[KeySerializer] = serializer.GetType().AssemblyQualifiedName;
        }

        private bool disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern impementation.
        /// </summary>
        /// <param name="disposing">Dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (activeConnection != null && activeConnection.State == ConnectionState.Open)
                    {
                        activeConnection.Close();
                        activeConnection.Dispose();
                        activeConnection = null;
                    }
                    if (serializer != null)
                    {
                        var disposableSerializer = serializer as IDisposable;
                        disposableSerializer?.Dispose();
                        serializer = null;
                    }
                }
                disposed = true;
            }
        }
    }
}
