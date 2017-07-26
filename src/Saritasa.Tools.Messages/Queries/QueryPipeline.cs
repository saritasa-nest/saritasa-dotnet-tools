// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using Saritasa.Tools.Messages.Internal;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Abstractions.Queries;
using Saritasa.Tools.Messages.Common;

namespace Saritasa.Tools.Messages.Queries
{
    /// <summary>
    /// Query pipeline.
    /// </summary>
    public class QueryPipeline : MessagePipeline, IQueryPipeline, IMessageRecordConverter
    {
        internal const string QueryParametersKey = ".query-parameters";

        private static readonly byte[] availableMessageTypes = { MessageContextConstants.MessageTypeQuery };

        /// <inheritdoc />
        public override byte[] MessageTypes => availableMessageTypes;

        /// <summary>
        /// Options.
        /// </summary>
        public new QueryPipelineOptions Options { get; set; } = new QueryPipelineOptions();

        private static QueryParameters CreateMessage(Delegate func, params object[] args)
        {
#if NET452
            var method = func.Method;
#else
            var method = func.GetInvocationList()[0].GetMethodInfo();
#endif

            return new QueryParameters
            {
                Parameters = args,
                Method = method,
                QueryObject = Activator.CreateInstance(method.GetBaseDefinition().DeclaringType, nonPublic: true),
                FakeQueryObject = true
            };
        }

        #region IQueryPipeline

        /// <inheritdoc />
        public virtual IQueryCaller<TQuery> Query<TQuery>(IMessagePipelineService pipelineService) where TQuery : class
        {
            var messageContext = new MessageContext(pipelineService);
            messageContext.ContentId = TypeHelpers.GetPartiallyAssemblyQualifiedName(typeof(TQuery));
            messageContext.Pipeline = this;
            return new QueryCaller<TQuery>(this, messageContext);
        }

        /// <inheritdoc />
        public virtual IQueryCaller<TQuery> Query<TQuery>(IMessagePipelineService pipelineService, TQuery obj) where TQuery : class
        {
            var messageContext = new MessageContext(pipelineService, obj);
            messageContext.ContentId = TypeHelpers.GetPartiallyAssemblyQualifiedName(typeof(TQuery));
            messageContext.Pipeline = this;
            return new QueryCaller<TQuery>(this, messageContext, obj);
        }

        /// <inheritdoc />
        public IQueryCaller<TQuery> CreateMessageContext<TQuery>(IMessagePipelineService pipelineService,
            IMessageContext messageContext) where TQuery : class
        {
            return new QueryCaller<TQuery>(this, messageContext).NoExecution();
        }

        #endregion

        #region IMessageRecordConverter

        /// <inheritdoc />
        public IMessageContext CreateMessageContext(IMessagePipelineService pipelineService, MessageRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }
            if (record.ContentType.IndexOf(".", StringComparison.Ordinal) < 0)
            {
                throw new ArgumentException(Properties.Strings.NoMethodNameFromContentType);
            }

            // Format object type name.
            var typesplit = record.ContentType.Split(','); // type.method, assembly
            var objectTypeName = typesplit[0].Substring(0, typesplit[0].LastIndexOf(".", StringComparison.Ordinal))
                + "," + typesplit[1];
            var objectType = Type.GetType(objectTypeName);

            if (objectType == null)
            {
                throw new InvalidOperationException(string.Format(Properties.Strings.CannotLoadType, objectTypeName));
            }
            var obj = Activator.CreateInstance(objectType, nonPublic: true);

            var methodName = typesplit[0].Substring(typesplit[0].LastIndexOf(".", StringComparison.Ordinal) + 1);
            var method = objectType.GetTypeInfo().GetMethod(methodName);
            if (method == null)
            {
                throw new InvalidOperationException(string.Format(Properties.Strings.CannotFindMethod, methodName));
            }
            var delegateType = Expression.GetDelegateType(
                method.GetParameters().Select(p => p.ParameterType).Concat(new[] { method.ReturnType }).ToArray());
            var @delegate = obj.GetType().GetTypeInfo().GetMethod(methodName).CreateDelegate(delegateType, obj);
            if (@delegate == null)
            {
                throw new InvalidOperationException(Properties.Strings.CannotCreateDelegate);
            }

            var messageContext = new MessageContext(pipelineService);
            messageContext.Pipeline = this;
            var dictContent = record.Content as IDictionary<string, object>;
            if (dictContent == null)
            {
                throw new ArgumentException(string.Format(Properties.Strings.ContentShouldBeType,
                    nameof(IDictionary<string, object>)));
            }
            var messageContent = dictContent.Values;
            var methodTypes = method.GetParameters().Select(p => p.ParameterType);
            var values = methodTypes.Zip(messageContent, (mt, mc) => TypeHelpers.ConvertType(mc, mt));

            var queryParameters = CreateMessage(@delegate, values.ToArray());
            messageContext.Items[QueryParametersKey] = queryParameters;
            messageContext.Content = dictContent;
            return messageContext;
        }

        /// <inheritdoc />
        public MessageRecord CreateMessageRecord(IMessageContext context)
        {
            var record = MessageRecordHelpers.Create(context);
            record.Type = MessageContextConstants.MessageTypeQuery;
            return record;
        }

        #endregion
    }
}
