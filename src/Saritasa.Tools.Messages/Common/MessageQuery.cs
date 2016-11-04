// Copyright (c) 2015-2016, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

namespace Saritasa.Tools.Messages.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Message query parameters.
    /// </summary>
    public class MessageQuery
    {
        /// <summary>
        /// Always return true delegate that accepts one parameter.
        /// </summary>
        public static readonly Expression<Func<object, bool>> ReturnTrueExpression = (obj) => true;

        /// <summary>
        /// Always return true delegate that accepts one parameter.
        /// </summary>
        public static readonly Expression<Func<Message, bool>> ReturnTrueMessageExpression = (obj) => true;

        /// <summary>
        /// Always return true delegate that accepts one parameter.
        /// </summary>
        public static readonly Expression<Func<IDictionary<string, string>, bool>> ReturnTrueDictExpression = (obj) => true;

        /// <summary>
        /// Message selector. Contain main message criterias.
        /// </summary>
        public Expression<Func<Message, bool>> MessageSelector { get; } = ReturnTrueMessageExpression;

        /// <summary>
        /// Has message message selector.
        /// </summary>
        public bool HasMessageSelector => MessageSelector != ReturnTrueMessageExpression;

        /// <summary>
        /// Message data selector. Data is just key-value string dictionary.
        /// </summary>
        public Expression<Func<IDictionary<string, string>, bool>> DataSelector { get; private set; } = ReturnTrueDictExpression;

        /// <summary>
        /// Has query data selector.
        /// </summary>
        public bool HasDataSelector => DataSelector != ReturnTrueDictExpression;

        /// <summary>
        /// Internal message structure selector.
        /// </summary>
        public LambdaExpression ContentSelector { get; private set; } = ReturnTrueExpression;

        /// <summary>
        /// Has query content selector.
        /// </summary>
        public bool HasContentSelector => ContentSelector != ReturnTrueExpression;

        /// <summary>
        /// Internal error structure selector.
        /// </summary>
        public LambdaExpression ErrorSelector { get; private set; } = ReturnTrueExpression;

        /// <summary>
        /// Has query error selector.
        /// </summary>
        public bool HasErrorSelector => ErrorSelector != ReturnTrueExpression;

        /// <summary>
        /// Assemblies to load types.
        /// </summary>
        public IList<Assembly> Assemblies { get; } = new List<Assembly>();

        /// <summary>
        /// How many messages to skip.
        /// </summary>
        public int Skip { get; private set; }

        /// <summary>
        /// How many record to return. Default is 1000.
        /// </summary>
        public int Take { get; private set; } = 1000;

        MessageQuery()
        {
        }

        MessageQuery(Expression<Func<Message, bool>> messageSelector)
        {
            if (messageSelector == null)
            {
                throw new ArgumentNullException(nameof(messageSelector));
            }
            MessageSelector = messageSelector;
        }

        /// <summary>
        /// Default create to retrieve all messages.
        /// </summary>
        /// <returns>Message query.</returns>
        public static MessageQuery Create()
        {
            return new MessageQuery();
        }

        /// <summary>
        /// Message query with message query.
        /// </summary>
        /// <param name="messageSelector">Message selector.</param>
        /// <returns>Message query.</returns>
        public static MessageQuery Create(Expression<Func<Message, bool>> messageSelector)
        {
            return new MessageQuery(messageSelector);
        }

        /// <summary>
        /// Add type loader assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies.</param>
        /// <returns>Message query.</returns>
        public MessageQuery WithAssemblies(Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assemblies.Add(assemblies[i]);
            }
            return this;
        }

        /// <summary>
        /// Add type loader assemblies.
        /// </summary>
        /// <param name="assemblies">Assemblies.</param>
        /// <returns>Message query.</returns>
        public MessageQuery WithAssemblies(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }
            foreach (var assembly in assemblies)
            {
                Assemblies.Add(assembly);
            }
            return this;
        }

#if !NETCOREAPP1_0 && !NETSTANDARD1_6
        /// <summary>
        /// Add calling assembly as type loader assembly.
        /// </summary>
        /// <returns>Message query.</returns>
        public MessageQuery WithCallingAssembly()
        {
            Assemblies.Add(Assembly.GetCallingAssembly());
            return this;
        }
#endif

        /// <summary>
        /// Set content selector.
        /// </summary>
        /// <param name="contentSelector">Content selector.</param>
        /// <returns>Message query.</returns>
        public MessageQuery WithContent(LambdaExpression contentSelector)
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }
            ContentSelector = contentSelector;
            return this;
        }

        /// <summary>
        /// Set content selector.
        /// </summary>
        /// <typeparam name="TContent">Content type.</typeparam>
        /// <param name="contentSelector">Content selector.</param>
        /// <returns>Message query.</returns>
        public MessageQuery WithContent<TContent>(Expression<Func<TContent, bool>> contentSelector) where TContent : class
        {
            if (contentSelector == null)
            {
                throw new ArgumentNullException(nameof(contentSelector));
            }
            ContentSelector = contentSelector;
            return this;
        }

        /// <summary>
        /// Set data selector.
        /// </summary>
        /// <param name="dataSelector">Data selector.</param>
        /// <returns>Message query.</returns>
        public MessageQuery WithData(Expression<Func<IDictionary<string, string>, bool>> dataSelector)
        {
            if (dataSelector == null)
            {
                throw new ArgumentNullException(nameof(dataSelector));
            }
            DataSelector = dataSelector;
            return this;
        }

        /// <summary>
        /// Set error selector.
        /// </summary>
        /// <param name="errorSelector">Error selector.</param>
        /// <returns>Message query.</returns>
        public MessageQuery WithError(LambdaExpression errorSelector)
        {
            if (errorSelector == null)
            {
                throw new ArgumentNullException(nameof(errorSelector));
            }
            ErrorSelector = errorSelector;
            return this;
        }

        /// <summary>
        /// Set error selector.
        /// </summary>
        /// <typeparam name="TError">Error type.</typeparam>
        /// <param name="errorSelector">Error selector.</param>
        /// <returns>Message query.</returns>
        public MessageQuery WithError<TError>(Expression<Func<TError, bool>> errorSelector) where TError : Exception
        {
            if (errorSelector == null)
            {
                throw new ArgumentNullException(nameof(errorSelector));
            }
            ErrorSelector = errorSelector;
            return this;
        }

        /// <summary>
        /// Restict output range.
        /// </summary>
        /// <param name="skip">How many messages to skip.</param>
        /// <param name="take">How many messages to take. Default is 1000.</param>
        /// <returns>Message query.</returns>
        public MessageQuery WithRange(int skip, int take = 1000)
        {
            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take));
            }
            if (skip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(skip));
            }

            Skip = skip;
            Take = take;
            return this;
        }
    }
}
