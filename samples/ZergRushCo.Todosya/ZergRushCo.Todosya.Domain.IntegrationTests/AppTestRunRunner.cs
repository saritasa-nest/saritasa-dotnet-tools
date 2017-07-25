// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System.Data.SqlClient;
using Autofac;
using Saritasa.Tools.Messages.TestRuns;
using System.Configuration;

namespace ZergRushCo.Todosya.Domain.IntegrationTests
{
    /// <summary>
    /// Application tests runner.
    /// </summary>
    public class AppTestRunRunner : TestRunRunner
    {
        protected string DatabaseSnapshotName => ConfigurationManager.AppSettings["TestRun:SnapshotDatabaseName"];

        protected string DatabaseSnapshotFile => ConfigurationManager.AppSettings["TestRun:SnapshotFile"];

        /// <inheritdoc />
        public override void OnInitialize(TestRunExecutionContext context)
        {
            var builder = new ContainerBuilder();
            ZergRushCo.Todosya.Infrastructure.DiConfig.Setup(builder, testingMode: true);

            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString))
            {
                sqlConnection.Open();
                var query =
                    $"DROP DATABASE IF EXISTS [{DatabaseSnapshotName}];" +
                    $"CREATE DATABASE [{DatabaseSnapshotName}] ON (NAME = [{sqlConnection.Database}], FILENAME = '{DatabaseSnapshotFile}') " +
                    $"AS SNAPSHOT OF [{sqlConnection.Database}];";
                RunQuery(query, sqlConnection);
            }
        }

        private int RunQuery(string sql, SqlConnection sqlConnection)
        {
            using (var sqlCommand = new SqlCommand(sql, sqlConnection))
            {
                return sqlCommand.ExecuteNonQuery();
            }
        }

        /// <inheritdoc />
        public override void OnBeforeTestRun(TestRunExecutionContext context)
        {
        }

        /// <inheritdoc />
        public override void OnShutdown(TestRunExecutionContext context)
        {
            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString))
            {
                sqlConnection.Open();
                RunQuery($"DROP DATABASE IF EXISTS [{DatabaseSnapshotName}];", sqlConnection);
            }
        }

        /// <inheritdoc />
        public override void OnAfterTestRun(TestRunExecutionContext context)
        {
            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString))
            {
                var originalDatabaseName = sqlConnection.Database;
                sqlConnection.Open();
                var query =
                    $"USE [master];" +
                    $"ALTER DATABASE [{originalDatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" +
                    $"RESTORE DATABASE [{originalDatabaseName}] FROM DATABASE_SNAPSHOT = '{DatabaseSnapshotName}';" +
                    $"ALTER DATABASE [{originalDatabaseName}] SET MULTI_USER;";
                RunQuery(query, sqlConnection);
            }
        }
    }
}
