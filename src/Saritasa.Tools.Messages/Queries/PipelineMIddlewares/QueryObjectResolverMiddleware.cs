// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Internal;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    /// <summary>
    /// Resolve and locate object handler for query.
    /// </summary>
    public class QueryObjectResolverMiddleware : BaseHandlerExecutorMiddleware
    {
        private readonly IDictionary<Type, Type> interfaceResolveDict =
            new Dictionary<Type, Type>();

        /// <summary>
        /// .ctor
        /// </summary>
        public QueryObjectResolverMiddleware()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="assemblies">Assemblies to search query handler.</param>
        public QueryObjectResolverMiddleware(params Assembly[] assemblies)
        {
            interfaceResolveDict = assemblies
                .SelectMany(a => a.GetTypes())
                .Select(t => t.GetTypeInfo())
                .Where(t => t.GetCustomAttribute<QueryHandlersAttribute>() != null)
                .SelectMany(t => t.GetInterfaces().Where(i => !i.FullName.StartsWith("System")).Select(i => new
                {
                    iface = i,
                    type = t
                }))
                .ToDictionary(
                    k => k.iface,
                    v => v.type.AsType()
                );
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
            var queryParams = messageContext.GetItemByKey<QueryParameters>(QueryPipeline.QueryParametersKey);

            var queryObjectType = queryParams.Method.DeclaringType;
            if (queryObjectType == null)
            {
                throw new InvalidOperationException("Query method does not have DeclaringType.");
            }
            if (queryObjectType.GetTypeInfo().IsInterface)
            {
                if (interfaceResolveDict.ContainsKey(queryObjectType))
                {
                    queryObjectType = interfaceResolveDict[queryObjectType];
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot find implementation for query with type {queryObjectType.FullName}.");
                }
            }
            if (queryParams.QueryObject == null)
            {
                if (!UseInternalObjectResolver && queryParams.Method.DeclaringType.GetTypeInfo().IsInterface)
                {
                    queryObjectType = queryParams.Method.DeclaringType;
                }
                queryParams.QueryObject = ResolveObject(queryObjectType, messageContext.ServiceProvider,
                    nameof(QueryObjectResolverMiddleware));
                if (UseInternalObjectResolver)
                {
                    messageContext.Items[QueryObjectReleaseMiddleware.IsInternalResolverUsedKey] = true;
                }
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

        private static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public override Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            Handle(messageContext);
            return completedTask;
        }
    }
}
