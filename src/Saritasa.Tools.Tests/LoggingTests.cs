using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Saritasa.Tools.Logging;

namespace Saritasa.Tools.Tests
{
    public class LoggingTests
    {
        [Test]
        public void IsLoggerFactorySupportScoped_should_work_correct()
        {
            var scopedLoggerFactory = new DummyScopedLoggerFactory();
            var loggerFactory = new DummyLoggerFactory();

            Assert.True(scopedLoggerFactory.IsLoggerFactorySupportScope(string.Empty));
            Assert.False(loggerFactory.IsLoggerFactorySupportScope(string.Empty));
        }

        [Test]
        public void ScopedLogger_shouldCorrectlyDisposeScope()
        {
            var scopedLoggerFactory = new DummyScopedLoggerFactory();
            var logger = scopedLoggerFactory.CreateScoped(string.Empty);

            using (logger.BeginScope("Scope"))
            {
                Assert.False(logger.Scope.IsDisposed);
            }

            Assert.True(logger.Scope.IsDisposed);
        }

        [Test]
        public void ScopedLogger_should_throw_exception_on_second_scope()
        {
            var scopedLoggerFactory = new DummyScopedLoggerFactory();
            var logger = scopedLoggerFactory.CreateScoped(string.Empty);

            using (logger.BeginScope("Scope"))
            {
                Assert.False(logger.Scope.IsDisposed);
                Assert.Throws<InvalidOperationException>(() => logger.BeginScope("Scope"));
            }
        }
    }
}
