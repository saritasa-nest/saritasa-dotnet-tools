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
    /// The middleware is to resolve and locate object handler for query.
    /// </summary>
    public class QueryObjectResolverMiddleware : BaseHandlerResolverMiddleware,
        IMessagePipelineMiddleware, IAsyncMessagePipelineMiddleware, IMessagePipelinePostAction
    {
        private readonly IDictionary<Type, Type> interfaceResolveDictionary =
            new Dictionary<Type, Type>();

        private readonly Action<IMessageContext> handleAction;

        /// <summary>
        /// Middleware identifier.
        /// </summary>
        public string Id { get; set; } = nameof(QueryObjectResolverMiddleware);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="useInternalObjectResolver">Use internal object resolver for handlers.
        /// Otherwise <see cref="IServiceProvider" /> will be used. <c>True</c> by default.</param>
        public QueryObjectResolverMiddleware(bool useInternalObjectResolver = true) :
            base(useInternalObjectResolver)
        {
            handleAction = useInternalObjectResolver ? HandleWithInternalResolver :
                (Action<IMessageContext>)HandleWithoutInternalResolver;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assemblies">Assemblies to search query handler. Should be used for internal resolver only.</param>
        public QueryObjectResolverMiddleware(params Assembly[] assemblies) : base(useInternalObjectResolver: true)
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

            interfaceResolveDictionary = allQueryTypes.ToDictionary(k => k.iface, v => v.type.AsType());
            handleAction = HandleWithInternalResolver;
        }

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            handleAction(messageContext);
        }

        private static Type GetDeclaringType(QueryParameters queryParameters)
        {
            var queryObjectType = queryParameters.Method.DeclaringType;
            if (queryObjectType == null)
            {
                throw new InvalidOperationException("Query method does not have DeclaringType.");
            }
            return queryObjectType;
        }

        private void HandleWithInternalResolver(IMessageContext messageContext)
        {
            var queryParameters = messageContext.GetItemByKey<QueryParameters>(QueryPipeline.QueryParametersKey);
            var queryObjectType = GetDeclaringType(queryParameters);

            // Resolve by interface.
            var isObjectTypeInterface = queryObjectType.GetTypeInfo().IsInterface;
            if (isObjectTypeInterface)
            {
                if (interfaceResolveDictionary.ContainsKey(queryObjectType))
                {
                    queryObjectType = interfaceResolveDictionary[queryObjectType];
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot find implementation for query with type {queryObjectType.FullName}.");
                }
            }

            // If query object still not resolved - we do that. For interfaces we substitute QueryObject with resolved one.
            if (queryParameters.QueryObject == null)
            {
                queryParameters.QueryObject = CreateHandlerWithCache(queryObjectType, messageContext.ServiceProvider);
            }
            if (queryParameters.QueryObject == null)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Strings.CannotResolveQueryObject, queryObjectType));
            }
        }

        private void HandleWithoutInternalResolver(IMessageContext messageContext)
        {
            var queryParameters = messageContext.GetItemByKey<QueryParameters>(QueryPipeline.QueryParametersKey);
            var queryObjectType = GetDeclaringType(queryParameters);

            // If query object still not resolved - we do that. For interfaces we substitute QueryObject with resolved one.
            if (queryParameters.QueryObject == null)
            {
                queryParameters.QueryObject = messageContext.ServiceProvider.GetService(queryObjectType);
            }
            if (queryParameters.QueryObject == null)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Strings.CannotResolveQueryObject, queryObjectType));
            }
        }

        private static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public virtual Task HandleAsync(IMessageContext messageContext, CancellationToken cancellationToken)
        {
            Handle(messageContext);
            return completedTask;
        }

        /// <inheritdoc />
        public void PostHandle(IMessageContext messageContext)
        {
            if (UseInternalObjectResolver)
            {
                var queryParams = messageContext.GetItemByKey<QueryParameters>(QueryPipeline.QueryParametersKey);
                var disposable = queryParams.QueryObject as IDisposable;
                disposable?.Dispose();
                queryParams.QueryObject = null;
            }
        }
    }
}
