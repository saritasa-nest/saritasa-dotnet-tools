using System;
using System.Linq;
using Xunit;
using ZergRushCo.Todosya.Domain.TaskContext.Commands;
using ZergRushCo.Todosya.Domain.TaskContext.Handlers;

namespace ZergRushCo.Todosya.Tests
{
    /// <summary>
    /// Tasks related tests.
    /// </summary>
    public class TasksTests
    {
        [Fact]
        public void Project_related_tasks_should_be_removed_on_project_remove()
        {
            var uowFactory = new AppTestUnitOfWorkFactory();
            uowFactory.SetSeedScenario1();

            // capture before
            int totalTasksCount = 0;
            using (var uow = uowFactory.Create())
            {
                int tasksProjectCount = uow.TaskRepository.GetAll().Count(t => t.Project.Id == 1);
                totalTasksCount = uow.TaskRepository.GetAll().Count();
                Assert.True(tasksProjectCount > 0);
                Assert.True(totalTasksCount > 0);
            }

            // remove project
            new ProjectHandlers().HandleRemoveProject(new RemoveProjectCommand()
            {
                ProjectId = 1,
                UpdatedByUserId = "1"
            }, uowFactory);

            // check after
            using (var uow = uowFactory.Create())
            {
                int tasksProjectCountAfter = uow.TaskRepository.GetAll().Count(t => t.Project.Id == 1);
                int totalTasksCountAfter = uow.TaskRepository.GetAll().Count();
                Assert.True(totalTasksCountAfter < totalTasksCount);
                Assert.Equal(0, tasksProjectCountAfter);
            }
        }
    }
}
