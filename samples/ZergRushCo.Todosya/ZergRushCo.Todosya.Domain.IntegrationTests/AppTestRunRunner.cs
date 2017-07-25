// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Configuration;
using System.Data.SqlClient;
using Autofac;
using Saritasa.Tools.Messages.TestRuns;
using ZergRushCo.Todosya.Infrastructure;

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
            DiConfig.Setup(builder, testingMode: true);
            var container = builder.Build();
            ServiceProviderFactory = new AutofacServiceProviderFactory(container);

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

        /// <inheritdoc />
        public override void OnBeforeTestRun(TestRunExecutionContext context)
        {
        }

        /// <inheritdoc />
        public override void OnShutdown(TestRunExecutionContext context)
        {
            
        }

        /// <inheritdoc />
        public override void OnAfterTestRun(TestRunExecutionContext context)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString))
            {
                sqlConnection.Open();

                var originalDatabaseName = sqlConnection.Database;
                var query =
                    $"USE [master];" +
                    $"ALTER DATABASE [{originalDatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" +
                    $"RESTORE DATABASE [{originalDatabaseName}] FROM DATABASE_SNAPSHOT = '{DatabaseSnapshotName}';" +
                    $"ALTER DATABASE [{originalDatabaseName}] SET MULTI_USER;";
                RunQuery(query, sqlConnection);
                RunQuery($"DROP DATABASE IF EXISTS [{DatabaseSnapshotName}];", sqlConnection);
            }
        }

        private int RunQuery(string sql, SqlConnection sqlConnection)
        {
            using (var sqlCommand = new SqlCommand(sql, sqlConnection))
            {
                return sqlCommand.ExecuteNonQuery();
            }
        }
    }
}
