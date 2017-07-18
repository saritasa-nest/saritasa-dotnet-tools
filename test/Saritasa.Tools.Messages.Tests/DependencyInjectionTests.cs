// Copyright (c) 2015-2017, Saritasa. All rights reserved.
// Licensed under the BSD license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Messages.Commands;
using Saritasa.Tools.Messages.Common;
using Xunit;

namespace Saritasa.Tools.Messages.Tests
{
    /// <summary>
    /// Dependency injection tests.
    /// </summary>
    public class DependencyInjectionTests
    {
        public interface IInterfaceA
        {
            string GetTestValue();
        }

        public class ImplementationA : IInterfaceA, IDisposable
        {
            public string GetTestValue() => "A";

            public static bool IsDisposed { get; set; }

            public ImplementationA()
            {
                IsDisposed = false;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        public static object InterfacesResolver(Type t)
        {
            if (t == typeof(IInterfaceA))
            {
                return new ImplementationA();
            }
            return null;
        }

        #region Should dispose command handlers after instaniate

        public class TestCommand
        {
        }

        [CommandHandlers]
        public class TestCommandHandler : IDisposable
        {
            public static bool IsDisposed { get; set; }

            public void HandleTestCommand(TestCommand command)
            {
                IsDisposed = false;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Fact]
        public void Should_dispose_command_handlers_after_instaniate()
        {
            // Arrange
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                    typeof(DependencyInjectionTests).GetTypeInfo().Assembly).UseInternalResolver();
            var cmd = new TestCommand();

            // Act
            cp.Handle(cmd);

            // Assert
            Assert.True(TestCommandHandler.IsDisposed);
        }

        [Fact]
        public async Task Should_dispose_command_handlers_after_instaniate_async()
        {
            // Arrange
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                typeof(DependencyInjectionTests).GetTypeInfo().Assembly).UseInternalResolver();
            var cmd = new TestCommand();

            // Act
            await cp.HandleAsync(cmd);

            // Assert
            Assert.True(TestCommandHandler.IsDisposed);
        }

        #endregion

        #region Should dispose resolved parameters after instaniate

        public class TestCommandA
        {
        }

        [CommandHandlers]
        public class TestCommandAHandler : IDisposable
        {
            public static bool IsDisposed { get; set; }

            public void HandleTestCommand(TestCommandA command, IInterfaceA a)
            {
                IsDisposed = false;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Fact]
        public void Should_dispose_resolved_parameters_after_instaniate()
        {
            // Arrange
            var cp = CommandPipeline.CreateDefaultPipeline(CommandPipeline.NullResolver,
                typeof(DependencyInjectionTests).GetTypeInfo().Assembly).UseInternalResolver();
            var cmd = new TestCommandA();

            // Act
            cp.Handle(cmd);

            // Assert
            Assert.True(ImplementationA.IsDisposed);
        }

        #endregion
    }
}
