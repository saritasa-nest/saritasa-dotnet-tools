// Copyright (c) 2015-2018, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    /// <summary>
    /// Resolve and locate object handler for query.
    /// </summary>
    public class QueryObjectResolverMiddleware : BaseHandlerResolverMiddleware,
        IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware, IMessagePipelinePostAction
    {
        private readonly IDictionary<Type, Type> interfaceResolveDict =
            new Dictionary<Type, Type>();

        /// <summary>
        /// Middleware identifier.
        /// </summary>
        public string Id { get; set; } = nameof(QueryObjectResolverMiddleware);

        /// <summary>
        /// .ctor
        /// </summary>
        public QueryObjectResolverMiddleware()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="assemblies">Assemblies to search query handler. Should be used for internal resolver only.</param>
        public QueryObjectResolverMiddleware(params Assembly[] assemblies)
        {
            var allQueryTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Select(t => t.GetTypeInfo())
                .Where(t => t.GetCustomAttribute<QueryHandlersAttribute>() != null)
                .SelectMany(t => t.GetInterfaces().Where(i => !i.FullName.StartsWith("System")).Select(i => new
                {
                    iface = i,
                    type = t
                }))
                .ToList();

            var implementationDuplicates = allQueryTypes
                .GroupBy(qt => qt.iface)
                .Where(qt => qt.Count() > 1)
                .ToList();
            if (implementationDuplicates.Any())
            {
                var duplicates = string.Join(", ", implementationDuplicates.Select(x => x.Key.FullName));
                throw new InvalidOperationException(string.Format(Properties.Strings.CannotResolveQueryMultipleTypes,
                    duplicates));
            }

            interfaceResolveDict = allQueryTypes.ToDictionary(k => k.iface, v => v.type.AsType());
        }

        /// <inheritdoc />
        public void Handle(IMessageContext messageContext)
        {
            var queryParams = messageContext.GetItemByKey<QueryParameters>(QueryPipeline.QueryParametersKey);

            var queryObjectType = queryParams.Method.DeclaringType;
            if (queryObjectType == null)
            {
                throw new InvalidOperationException("Query method does not have DeclaringType.");
            }

            // Resolve by interface.
            if (UseInternalObjectResolver && queryObjectType.GetTypeInfo().IsInterface)
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

            // If query object still not resolved - we do that. For interfaces we substitute QueryObject with resolved one.
            if (queryParams.QueryObject == null)
            {
                queryParams.QueryObject = UseInternalObjectResolver ?
                    CreateHandlerWithCache(queryObjectType, messageContext.ServiceProvider, Id) :
                    messageContext.ServiceProvider.GetService(queryObjectType);
            }
            if (queryParams.QueryObject == null)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Strings.CannotResolveQueryObject, queryObjectType));
            }
        }

        private static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            Handle(messageContext);
            return completedTask;
        }

        /// <inheritdoc />
        public void PostHandle(IMessageContext messageContext)
        {
            if (UseInternalObjectResolver)
            {
                var queryParams = (QueryParameters)messageContext.Items[QueryPipeline.QueryParametersKey];
                var disposable = queryParams.QueryObject as IDisposable;
                disposable?.Dispose();
                queryParams.QueryObject = null;
            }
        }
    }
}
