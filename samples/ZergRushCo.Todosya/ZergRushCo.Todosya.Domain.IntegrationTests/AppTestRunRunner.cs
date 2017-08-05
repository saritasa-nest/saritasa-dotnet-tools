// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Configuration;
using System.Data.SqlClient;
using Autofac;
using Saritasa.Tools.Messages.TestRuns;
using ZergRushCo.Todosya.DataAccess;
using ZergRushCo.Todosya.Infrastructure;

namespace ZergRushCo.Todosya.Domain.IntegrationTests
{
    /// <summary>
    /// Application tests runner.
    /// </summary>
    public class AppTestRunRunner : TestRunRunner
    {
        private IContainer container;

        /// <inheritdoc />
        public override void OnInitialize(TestRunExecutionContext context)
        {
            var builder = new ContainerBuilder();
            DiConfig.Setup(builder, testingMode: true);
            container = builder.Build();
            ServiceProviderFactory = new AutofacServiceProviderFactory(container);
        }

        /// <inheritdoc />
        public override void OnBeforeTestRun(TestRunExecutionContext context)
        {
            using (var sqlConnection = new SqlConnection(GetConnectionString()))
            {
                var dbContext = new AppDbContext(sqlConnection);
                dbContext.Database.CreateIfNotExists();
            }
        }

        /// <inheritdoc />
        public override void OnShutdown(TestRunExecutionContext context)
        {
        }

        /// <inheritdoc />
        public override void OnAfterTestRun(TestRunExecutionContext context)
        {
            using (var sqlConnection = new SqlConnection(GetConnectionString()))
            {
                var dbContext = new AppDbContext(sqlConnection);
                dbContext.Database.Delete();
            }
        }

        private string GetConnectionString(string database = null)
        {
            var cs = ConfigurationManager.ConnectionStrings["AppDbContext"].ConnectionString;
            if (!string.IsNullOrEmpty(database))
            {
                var csBuilder = new SqlConnectionStringBuilder(cs)
                {
                    InitialCatalog = database
                };
                cs = csBuilder.ConnectionString;
            }
            return cs;
        }
    }
}
