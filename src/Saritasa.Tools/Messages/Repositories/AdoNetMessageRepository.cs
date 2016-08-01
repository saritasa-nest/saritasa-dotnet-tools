﻿// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using ObjectSerializers;
    using QueryProviders;
    using Internal;
    using System.Text;

    /// <summary>
    /// Use ADO.NET infrastructure to store commands.
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
            SqLite,

            /// <summary>
            /// Oracle.
            /// </summary>
            Oracle,
        }

        Dialect dialect = Dialect.MySql;

        bool isInitialized = false;

        IDbConnection activeConnection;

        DbProviderFactory factory;

        bool keepConnection;

        string connectionString;

        IObjectSerializer serializer;

        IMessageQueryProvider queryProvider;

        object objLock = new object();

        /// <summary>
        /// Keep connection opened between queries. False by default.
        /// </summary>
        public bool KeepConnection
        {
            get { return keepConnection; }
            set { keepConnection = value; }
        }

        /// <summary>
        /// Ado.Net repository.
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
            this.serializer = serializer != null ? serializer : new JsonObjectSerializer();
            this.queryProvider = CreateSqlProvider(this.dialect, this.serializer);
        }

        private static IMessageQueryProvider CreateSqlProvider(Dialect dialect, IObjectSerializer serializer)
        {
            switch (dialect)
            {
                case Dialect.Auto:
                    throw new NotImplementedException($"The sql provider {dialect} is not implemented yet");
                case Dialect.MySql:
                    return new MySqlQueryProvider(serializer);
                case Dialect.SqlServer:
                    return new SqlServerQueryProvider(serializer);
                default:
                    throw new NotImplementedException($"The sql provider {dialect} is not implemented yet");
            }
        }

        /// <inheritdoc />
        public void Add(Message result)
        {
            if (isInitialized == false)
            {
                Init();
            }

            IDbConnection connection = null;
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
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (keepConnection && connection != null)
                {
                    connection.Close();
                    connection = null;
                }
            }
        }

        private void AddParameter(IDbCommand cmd, string name, object value)
        {
            var param = factory.CreateParameter();
            param.ParameterName = name;
            if (value is byte[] && serializer.IsText)
            {
                value = System.Text.Encoding.UTF8.GetString((byte[])value);
            }
            param.Value = value != null ? value : DBNull.Value;
            param.Direction = ParameterDirection.Input;
            cmd.Parameters.Add(param);
        }

        private void Init()
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
                if (keepConnection && connection != null)
                {
                    connection.Close();
                    connection = null;
                }
            }
        }

        private IDbConnection GetConnection()
        {
            lock (objLock)
            {
                if (keepConnection)
                {
                    if (activeConnection == null || activeConnection.State != ConnectionState.Open)
                    {
                        activeConnection = factory.CreateConnection();
                        activeConnection.ConnectionString = connectionString;
                        activeConnection.Open();
                    }
                    return activeConnection;
                }
                else
                {
                    var connection = factory.CreateConnection();
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    return connection;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<Message> Get(MessageQuery messageQuery)
        {
            // execute
            IList<Message> messages = new List<Message>();
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = queryProvider.GetFilterScript(messageQuery);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var message = new Message();
                        message.Type = reader.GetByte(1);
                        message.Id = reader.GetGuid(2);
                        message.ContentType = reader.GetString(3);
                        var content = serializer.IsText ? Encoding.UTF8.GetBytes(reader.GetString(4)) : (byte[])reader[4];
                        TypeHelpers.ResolveTypeForContent(message, content, serializer, messageQuery.Assemblies.ToArray());
                        message.Data = serializer.IsText ?
                            (IDictionary<string, string>)serializer.Deserialize(Encoding.UTF8.GetBytes(reader.GetString(5)), typeof(IDictionary<string, string>)) :
                            (IDictionary<string, string>)serializer.Deserialize(Encoding.UTF8.GetBytes(reader.GetString(5)), typeof(IDictionary<string, string>));
                        var error = serializer.IsText ? Encoding.UTF8.GetBytes(reader.GetString(7)) : (byte[])reader[7];
                        TypeHelpers.ResolveTypeForError(message, error, serializer, messageQuery.Assemblies.ToArray());
                        message.ErrorMessage = reader.GetString(7);
                        message.ErrorType = reader.GetString(8);
                        message.CreatedAt = reader.GetDateTime(9);
                        message.ExecutionDuration = reader.GetInt32(10);
                        message.Status = (Message.ProcessingStatus)reader.GetByte(11);

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
            dict[nameof(keepConnection)] = keepConnection;
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
#if NETCOREAPP1_0 || NETSTANDARD1_6
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

        /// <inheritdoc />
        public void Dispose()
        {
            if (activeConnection != null && activeConnection.State == ConnectionState.Open)
            {
                activeConnection.Close();
                activeConnection = null;
            }
        }
    }
}
