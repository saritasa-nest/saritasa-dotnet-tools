// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq.Expressions;
    using System.Reflection;
    using ObjectSerializers;
    using SqlProviders;

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

        ISqlProvider sqlProvider;

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
            this.sqlProvider = CreateSqlProvider(this.dialect, this.serializer);
        }

        private static ISqlProvider CreateSqlProvider(Dialect dialect, IObjectSerializer serializer)
        {
            switch (dialect)
            {
                case Dialect.Auto:
                    throw new NotImplementedException($"The sql provider {dialect} is not implemented yet");
                case Dialect.MySql:
                    return new MySqlSqlProvider(serializer);
                case Dialect.SqlServer:
                    return new SqlServerSqlProvider(serializer);
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
                    command.CommandText = sqlProvider.GetInsertMessageScript();
                    AddParameter(command, "@Type", result.Type);
                    AddParameter(command, "@ContentId", result.Id.ToString().Replace("-", string.Empty));
                    AddParameter(command, "@ContentType", result.ContentType);
                    AddParameter(command, "@Content", serializer.Serialize(result.Content));
                    AddParameter(command, "@Data", result.Data != null ? serializer.Serialize(result.Data) : null);
                    AddParameter(command, "@ErrorDetails", result.ErrorDetails != null ? serializer.Serialize(result.ErrorDetails) : null);
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
                    command.CommandText = sqlProvider.GetExistsTableScript();
                    if (command.ExecuteScalar() == null)
                    {
                        command.CommandText = sqlProvider.GetCreateTableScript();
                        int a = command.ExecuteNonQuery();
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
        public IEnumerable<Message> Get(Expression<Func<Message, bool>> selector, Assembly[] assemblies = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SaveState(IDictionary<string, object> dict)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // TODO
        }
    }
}
