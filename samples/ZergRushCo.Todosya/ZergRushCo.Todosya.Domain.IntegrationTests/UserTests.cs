using System;
using Saritasa.Tools.Domain.Exceptions;
using Saritasa.Tools.Messages.TestRuns.Loaders;
using Xunit;

namespace ZergRushCo.Todosya.Domain.IntegrationTests
{
    /// <summary>
    /// Users tests.
    /// </summary>
    public class UserTests : IDisposable
    {
        private readonly AppTestRunRunner runner = new AppTestRunRunner();

        [Fact]
        public void Cannot_create_user_with_equal_email()
        {
            using (var stream = TestHelpers.GetManifestStream(@"Users.Cannot_create_user_with_equal_email.json"))
            {
                var result = runner.Run(new StreamLoader(stream));
                Assert.False(result.IsSuccess);
                Assert.IsType<DomainException>(result.FailException.InnerException);
            }
        }

        [Fact]
        public void Cannot_edit_project_for_another_user()
        {
            using (var stream = TestHelpers.GetManifestStream(@"Users.Cannot_edit_project_for_another_user.json"))
            {
                var result = runner.Run(new StreamLoader(stream));
                Assert.False(result.IsSuccess);
                Assert.IsType<ForbiddenException>(result.FailException.InnerException);
            }
        }

        public void Dispose()
        {
            runner?.Dispose();
        }
    }
}
