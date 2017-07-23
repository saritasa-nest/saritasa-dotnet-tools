// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Internal;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    /// <summary>
    /// Resolve object handler for query.
    /// </summary>
    public class QueryObjectResolverMiddleware : BaseExecutorMiddleware
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public QueryObjectResolverMiddleware()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="parameters">Dictionary with parameters.</param>
        public QueryObjectResolverMiddleware(IDictionary<string, string> parameters) : base(parameters)
        {
        }

        /// <inheritdoc />
        public override void Handle(IMessageContext messageContext)
        {
            var queryParams = (QueryParameters)messageContext.Items[QueryPipeline.QueryParametersKey];

            var queryObjectType = queryParams.QueryObject.GetType();
            if (queryParams.FakeQueryObject)
            {
                queryParams.QueryObject = ResolveObject(queryObjectType, messageContext.ServiceProvider,
                    nameof(QueryObjectResolverMiddleware));
            }
            if (queryParams.QueryObject == null)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Strings.CannotResolveQueryObject, queryObjectType));
            }
            if (UseParametersResolve)
            {
                TypeHelpers.ResolveForParameters(queryParams.Parameters, queryParams.Method.GetParameters(),
                    messageContext.ServiceProvider.GetService);
            }
        }

        static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public override Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            Handle(messageContext);
            return completedTask;
        }
    }
}
