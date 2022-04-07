// Copyright (c) 2015-2020, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using Saritasa.Tools.Common.Utils;
using System;
using Xunit;

namespace Saritasa.Tools.Common.Tests
{
    /// <summary>
    /// Object utils tests.
    /// </summary>
    public class ObjectUtilsTests
    {
        internal interface IDependency
        {
        }

        internal class Dependency : IDependency
        {
        }

        internal class ServiceProvider : IServiceProvider
        {
            /// <inheritdoc />
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IDependency))
                {
                    return new Dependency();
                }
                return null;
            }
        }

        internal class ClassToCreate
        {
            public IDependency Dependency { get; }

            public int A { get; }

            public object B { get; }

            public string S { get; }

            public ClassToCreate(IDependency dependency, int a = 10, object b = null, string s = "test")
            {
                Dependency = dependency;
                A = a;
                B = b;
                S = s;
            }
        }
    }
}
