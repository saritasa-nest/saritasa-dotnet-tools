// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Saritasa.Tools.Messages.Abstractions;
using JetBrains.Annotations;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Locates handler base middleware.
    /// </summary>
    public abstract class BaseHandlerLocatorMiddleware : IMessagePipelineMiddleware
    {
        /// <inheritdoc />
        public string Id { get; set; } = "Locator";

        /// <summary>
        /// Assemblies to search in.
        /// </summary>
        protected Assembly[] Assemblies { get; set; } = new Assembly[0];

        /// <summary>
        /// .ctor
        /// </summary>
        protected BaseHandlerLocatorMiddleware()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="dict">Input parameters as dict.</param>
        public BaseHandlerLocatorMiddleware(IDictionary<string, string> dict)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (dict.ContainsKey("id"))
            {
                Id = dict["id"];
            }
            if (dict.ContainsKey("assemblies"))
            {
                var assemblies = new List<Assembly>();
                foreach (string assemblyFile in dict["assemblies"].Split(';'))
                {
                    Assembly assembly;
#if NETCOREAPP1_0 || NETCOREAPP1_1 || NETSTANDARD1_2 || NETSTANDARD1_6
                    assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
#else
                    assembly = Assembly.LoadFrom(assemblyFile);
#endif
                    assemblies.Add(assembly);
                }
                this.Assemblies = assemblies.ToArray();
            }
            Initialize();
        }

        HandlerSearchMethod handlerSearchMethod = HandlerSearchMethod.ClassAttribute;

        /// <summary>
        /// What method to use to search command handler class.
        /// </summary>
        public HandlerSearchMethod HandlerSearchMethod
        {
            get
            {
                return handlerSearchMethod;
            }

            set
            {
                if (handlerSearchMethod != value)
                {
                    handlerSearchMethod = value;
                    Initialize();
                }
            }
        }

        /// <inheritdoc />
        public abstract void Handle([NotNull] IMessage message);

        /// <summary>
        /// Initialize middleware.
        /// </summary>
        protected abstract void Initialize();
    }
}
