// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Queries.QueryPipelineMIddlewares
{
    using System;
    using System.Reflection;
    using Internal;
    using Messages;

    /// <summary>
    /// Resolve object for query.
    /// </summary>
    public class QueryObjectResolverMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "QueryObjectResolver";

        readonly Func<Type, object> resolver;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="resolver">Resolver func.</param>
        public QueryObjectResolverMiddleware(Func<Type, object> resolver)
        {
            this.resolver = resolver;
        }

        /// <inheritdoc />
        public void Handle(Message message)
        {
            var queryMessage = message as QueryMessage;
            if (queryMessage == null)
            {
                throw new NotSupportedException("Message should be QueryMessage type");
            }

            queryMessage.QueryObject = TypeHelpers.ResolveObjectForType(queryMessage.Func.GetMethodInfo().DeclaringType, resolver,
                nameof(QueryObjectResolverMiddleware));
            if (queryMessage.QueryObject == null)
            {
                queryMessage.QueryObject = resolver.Target;
            }

            TypeHelpers.ResolveForParameters(queryMessage.Parameters, queryMessage.Func.GetMethodInfo().GetParameters(), resolver);
        }
    }
}
