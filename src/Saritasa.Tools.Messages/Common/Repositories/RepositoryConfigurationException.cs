// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;

namespace Saritasa.Tools.Messages.Common.Repositories
{
    /// <summary>
    /// Repository configuration exception.
    /// </summary>
#if !NETCOREAPP1_0 && !NETCOREAPP1_1 && !NETSTANDARD1_6
    [Serializable]
#endif
    public class RepositoryConfigurationException : Exception
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public RepositoryConfigurationException()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="message">Exception message.</param>
        public RepositoryConfigurationException(string message) : base(message)
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="innerException">Inner exception.</param>
        public RepositoryConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Throws exception when create repository by calling ctor with dictionary argument.
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        public static void ThrowParameterNotExists(string parameterName)
        {
            throw new RepositoryConfigurationException($"Expected parameter for reposiotry {parameterName}.");
        }
    }
}
