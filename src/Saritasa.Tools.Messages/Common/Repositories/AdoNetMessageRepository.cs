// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Abstractions;
    using ObjectSerializers;
    using QueryProviders;
    using Internal;

    /// <summary>
    /// Use ADO.NET infrastructure to store messages.
    /// </summary>
    public class AdoNetMessageRepository : IMessageRepository, IDisposable
    {
        /// <summary>
        /// Possible sql dialects.
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

        readonly Dialect dialect;

        bool isInitialized;

        DbConnection activeConnection;

        readonly DbProviderFactory factory;

        readonly string connectionString;

        IObjectSerializer serializer;

        readonly IMessageQueryProvider queryProvider;

        readonly object objLock = new object();

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
            this.dialect = dialect;
            this.connectionString = connectionString;
            this.factory = factory;
            this.serializer = serializer ?? new JsonObjectSerializer();
            this.queryProvider = CreateSqlProvider(this.dialect, this.factory, this.serializer);
        }

        static IMessageQueryProvider CreateSqlProvider(Dialect dialect, DbProviderFactory factory, IObjectSerializer serializer)
        {
            switch (dialect)
            {
                case Dialect.Auto:
                    var provider = CreateSqlProviderByFactory(factory, serializer);
                    if (provider == null)
                    {
                        throw new NotImplementedException($"The sql provider for factory {factory} is not implemented yet");
                    }
                    return provider;
                case Dialect.MySql:
                    return new MySqlQueryProvider(serializer);
                case Dialect.SqlServer:
                    return new SqlServerQueryProvider(serializer);
                case Dialect.Sqlite:
                    return new SqliteQueryProvider(serializer);
                default:
                    throw new NotImplementedException($"The sql provider {dialect} is not implemented yet");
            }
        }

        static IMessageQueryProvider CreateSqlProviderByFactory(DbProviderFactory factory, IObjectSerializer serializer)
        {
            var name = factory.GetType().Name.ToLowerInvariant();
            if (name.Contains("sqlclient"))
            {
                return new SqlServerQueryProvider(serializer);
            }
            if (name.Contains("mysql"))
            {
                return new MySqlQueryProvider(serializer);
            }
            if (name.Contains("sqlite"))
            {
                return new SqliteQueryProvider(serializer);
            }
            return null;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities",
            Justification = "Parameters are used")]
        public async Task AddAsync(IMessage result)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AdoNetMessageRepository));
            }
            if (!isInitialized)
            {
                Init();
                isInitialized = true;
            }

            DbConnection connection = null;
            try
            {
                connection = GetConnection();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = queryProvider.GetInsertMessageScript();
                    AddParameter(command, "@Type", result.Type);
                    AddParameter(command, "@ContentId", result.Id.ToString());
                    AddParameter(command, "@ContentType", result.ContentType);
                    AddParameter(command, "@Content", serializer.Serialize(result.Content));
                    AddParameter(command, "@Data", result.Data != null ? serializer.Serialize(result.Data) : null);
                    AddParameter(command, "@ErrorDetails", result.Error != null ? serializer.Serialize(result.Error) : null);
                    AddParameter(command, "@ErrorMessage", result.ErrorMessage);
                    AddParameter(command, "@ErrorType", result.ErrorType);
                    AddParameter(command, "@CreatedAt", result.CreatedAt);
                    AddParameter(command, "@ExecutionDuration", result.ExecutionDuration);
                    AddParameter(command, "@Status", (byte)result.Status);
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                if (KeepConnection && connection != null)
                {
                    connection.Close();
                    connection = null;
                }
            }
        }

        void AddParameter(IDbCommand cmd, string name, object value)
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
        void Init()
        {
            IDbConnection connection = null;
            try
            {
                connection = GetConnection();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = queryProvider.GetExistsTableScript();
                    if (command.ExecuteScalar() == null)
                    {
                        command.CommandText = queryProvider.GetCreateTableScript();
                        command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                if (KeepConnection && connection != null)
                {
                    connection.Close();
                    connection = null;
                }
            }
        }

        DbConnection GetConnection()
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
        public async Task<IEnumerable<IMessage>> GetAsync(MessageQuery messageQuery)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(AdoNetMessageRepository));
            }

            // execute
            IList<Message> messages = new List<Message>();
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = queryProvider.GetFilterScript(messageQuery);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var message = new Message()
                        {
                            Type = reader.GetByte(1),
                            Id = reader.GetGuid(2),
                            ContentType = reader.GetString(3)
                        };
                        var content = serializer.IsText ? Encoding.UTF8.GetBytes(reader.GetString(4)) : (byte[])reader[4];
                        TypeHelpers.ResolveTypeForContent(message, content, serializer, messageQuery.Assemblies.ToArray());
                        message.Data = (IDictionary<string, string>)serializer.Deserialize(Encoding.UTF8.GetBytes(reader.GetString(5)), typeof(IDictionary<string, string>));
                        var error = serializer.IsText ? Encoding.UTF8.GetBytes(reader.GetString(7)) : (byte[])reader[7];
                        TypeHelpers.ResolveTypeForError(message, error, serializer, messageQuery.Assemblies.ToArray());
                        message.ErrorMessage = reader.GetString(7);
                        message.ErrorType = reader.GetString(8);
                        message.CreatedAt = reader.GetDateTime(9);
                        message.ExecutionDuration = reader.GetInt32(10);
                        message.Status = (ProcessingStatus)reader.GetByte(11);

                        messages.Add(message);
                    }
                }
            }

            return messages;
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, object> dict)
        {
            dict[nameof(dialect)] = dialect;
            dict[nameof(KeepConnection)] = KeepConnection;
            dict[nameof(factory)] = factory.GetType().Namespace;
            dict[nameof(connectionString)] = connectionString;
            dict[nameof(serializer)] = serializer.GetType().AssemblyQualifiedName;
        }

        /// <summary>
        /// Create repository from dictionary.
        /// </summary>
        /// <param name="dict">Properties.</param>
        /// <returns>Message repository.</returns>
        public static IMessageRepository CreateFromState(IDictionary<string, object> dict)
        {
#if NETCOREAPP1_1 || NETSTANDARD1_6
            throw new NotSupportedException("Not sure how to handle DbProviderFactories for .NET Core");
#else
            return new AdoNetMessageRepository(
                DbProviderFactories.GetFactory(dict[nameof(factory)].ToString()),
                dict[nameof(connectionString)].ToString(),
                (Dialect)Enum.Parse(typeof(Dialect), dict[nameof(dialect)].ToString(), true),
                (IObjectSerializer)Activator.CreateInstance(Type.GetType(dict[nameof(serializer)].ToString()))
            );
#endif
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
