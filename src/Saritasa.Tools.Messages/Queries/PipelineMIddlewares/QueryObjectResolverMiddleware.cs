// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    using System;
    using Internal;
    using Common;

    /// <summary>
    /// Resolve object handler for query.
    /// </summary>
    public class QueryObjectResolverMiddleware : BaseExecutorMiddleware
    {
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolver">Resolver func.</param>
        public QueryObjectResolverMiddleware(Func<Type, object> resolver) : base(resolver)
        {
            Id = "QueryResolver";
        }

        /// <inheritdoc />
        public override void Handle(Message message)
        {
            var queryMessage = message as QueryMessage;
            if (queryMessage == null)
            {
                throw new NotSupportedException("Message should be QueryMessage type");
            }

            if (queryMessage.FakeQueryObject)
            {
                queryMessage.QueryObject = ResolveObject(queryMessage.QueryObject.GetType(), nameof(QueryObjectResolverMiddleware));
            }
            if (queryMessage.QueryObject == null)
            {
                throw new InvalidOperationException($"Query object of type {queryMessage.QueryObject.GetType()} cannot be resolved");
            }
            if (UseParametersResolve)
            {
                TypeHelpers.ResolveForParameters(queryMessage.Parameters, queryMessage.Method.GetParameters(), Resolver);
            }
        }
    }
}
