// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saritasa.Tools.Messages.Common
{
    /// <summary>
    /// Messages repository factory.
    /// </summary>
    public static class RepositoryFactory
    {
        /// <summary>
        /// Create repository by type name and parameters.
        /// </summary>
        /// <param name="typeName">Type name.</param>
        /// <param name="parameters">Parameters dicionary.</param>
        /// <returns>Message repository.</returns>
        public static IMessageRepository CreateFromTypeName(string typeName, IDictionary<string, string> parameters)
        {
            var repositoryType = Type.GetType(typeName);
            if (repositoryType == null)
            {
                throw new ArgumentException($"Cannot load repository type {typeName}.");
            }

            var ctor = repositoryType.GetTypeInfo()
                .GetConstructors()
                .FirstOrDefault(c => c.GetParameters().Length == 1
                                    && c.GetParameters()[0].ParameterType == typeof(IDictionary<string, string>));
            if (ctor == null)
            {
                ctor = repositoryType.GetTypeInfo().GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 0);
            }

            if (ctor == null)
            {
                var msg = "Cannot find public parameterless constructor or constructor that accepts IDictionary<string, string>.";
                throw new InvalidOperationException(msg);
            }

            var repository = ctor.GetParameters().Length == 1
                ? ctor.Invoke(new object[] { parameters }) as IMessageRepository
                : ctor.Invoke(new object[] { }) as IMessageRepository;
            if (repository == null)
            {
                throw new InvalidOperationException($"Cannot instaniate repository {repositoryType.Name}.");
            }
            return repository;
        }
    }
}
