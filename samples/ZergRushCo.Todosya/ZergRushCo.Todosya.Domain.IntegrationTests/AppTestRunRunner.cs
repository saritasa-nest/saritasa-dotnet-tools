// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Transactions;
using Autofac;
using Saritasa.Tools.Messages.TestRuns;

namespace ZergRushCo.Todosya.Domain.IntegrationTests
{
    /// <summary>
    /// Application tests runner.
    /// </summary>
    public class AppTestRunRunner : TestRunRunner
    {
        private TransactionScope transactionScope;

        /// <inheritdoc />
        public override void Setup(TestRunExecutionContext context)
        {
            var builder = new ContainerBuilder();
            ZergRushCo.Todosya.Infrastructure.DiConfig.Setup(builder);
            context.SetResolver(builder.Build().Resolve);
        }

        /// <inheritdoc />
        public override void OnBeforeTestRun(TestRunExecutionContext context)
        {
            transactionScope?.Dispose();
            var transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;
            transactionOptions.Timeout = TimeSpan.FromSeconds(60);
            transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }

        /// <inheritdoc />
        public override void OnAfterTestRun(TestRunExecutionContext context)
        {
            transactionScope.Dispose();
            transactionScope = null;
        }
    }
}
