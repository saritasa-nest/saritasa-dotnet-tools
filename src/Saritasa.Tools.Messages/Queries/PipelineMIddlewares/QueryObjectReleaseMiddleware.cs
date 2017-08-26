// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using Saritasa.Tools.Messages.Abstractions;

namespace Saritasa.Tools.Messages.Queries.PipelineMiddlewares
{
    /// <summary>
    /// Dispose QueryObject of message if possible.
    /// </summary>
    public class QueryObjectReleaseMiddleware : IMessagePipelineMiddleware
    {
        internal const string IsInternalResolverUsedKey = "internal-resolver-used";

        /// <inheritdoc />
        public string Id { get; set; } = nameof(QueryObjectReleaseMiddleware);

        /// <inheritdoc />
        public virtual void Handle(IMessageContext messageContext)
        {
            var queryParams = (QueryParameters)messageContext.Items[QueryPipeline.QueryParametersKey];

            // Release handler.
            if (messageContext.Items.ContainsKey(IsInternalResolverUsedKey))
            {
                var disposable = queryParams.QueryObject as IDisposable;
                disposable?.Dispose();
            }
            queryParams.QueryObject = null;
        }
    }
}
