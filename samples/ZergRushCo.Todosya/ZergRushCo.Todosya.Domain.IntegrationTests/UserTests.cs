using System;
using Saritasa.Tools.Domain.Exceptions;
using Saritasa.Tools.Messages.TestRuns;
using Xunit;

namespace ZergRushCo.Todosya.Domain.IntegrationTests
{
    /// <summary>
    /// Users tests.
    /// </summary>
    public class UserTests : IDisposable
    {
        private readonly AppTestRunRunner runner = new AppTestRunRunner();

        [Theory]
        [TestRunFile("Users.Cannot_create_user_with_equal_email")]
        public void Cannot_create_user_with_equal_email(TestRun testRun)
        {
            var result = runner.Run(testRun);
            Assert.False(result.IsSuccess);
            Assert.IsType<DomainException>(result.FailException.InnerException);
        }

        [Theory]
        [TestRunFile("Users.Cannot_edit_project_for_another_user")]
        public void Cannot_edit_project_for_another_user(TestRun testRun)
        {
            var result = runner.Run(testRun);
            Assert.False(result.IsSuccess);
            Assert.IsType<ForbiddenException>(result.FailException.InnerException);
        }

        public void Dispose()
        {
            runner?.Dispose();
        }
    }
}
