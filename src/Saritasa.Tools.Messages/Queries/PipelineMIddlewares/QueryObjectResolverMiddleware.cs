// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    using System;
    using System.Threading.Tasks;
    using Abstractions;
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
        public override void Handle(IMessage message)
        {
            var queryMessage = message as QueryMessage;
            if (queryMessage == null)
            {
                throw new NotSupportedException(string.Format(Properties.Strings.MessageShouldBeType,
                    nameof(QueryMessage)));
            }

            var queryObjectType = queryMessage.QueryObject.GetType();
            if (queryMessage.FakeQueryObject)
            {
                queryMessage.QueryObject = ResolveObject(queryObjectType, nameof(QueryObjectResolverMiddleware));
            }
            if (queryMessage.QueryObject == null)
            {
                throw new InvalidOperationException(
                    string.Format(Properties.Strings.CannotResolveQueryObject, queryObjectType));
            }
            if (UseParametersResolve)
            {
                TypeHelpers.ResolveForParameters(queryMessage.Parameters, queryMessage.Method.GetParameters(), Resolver);
            }
        }

        static readonly Task<bool> completedTask = Task.FromResult(true);

        /// <inheritdoc />
        public override Task HandleAsync(IMessage message)
        {
            Handle(message);
            return completedTask;
        }
    }
}
