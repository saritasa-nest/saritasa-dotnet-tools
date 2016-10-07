using System;
using Saritasa.Tools.Commands;
using Saritasa.Tools.Exceptions;
using ZergRushCo.Todosya.Domain.Tasks.Commands;

namespace ZergRushCo.Todosya.Domain.Tasks.Handlers
{
    [CommandHandlers]
    public class TaskHandlers
    {
        public void HandleCreateTask(CreateTaskCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var project = uow.ProjectRepository.Get(command.ProjectId);
                if (project != null && project.User.Id != command.UserId)
                {
                    throw new DomainException("User does not own the project");
                }

                var user = uow.UserRepository.Get(command.UserId);

                var task = new Entities.Task()
                {
                    DueDate = DateTime.MaxValue,
                    IsDone = command.IsDone,
                    Text = command.Text.Trim(),
                    Project = project,
                    User = user,
                };
                uow.TaskRepository.Add(task);
                uow.Complete();
                command.TaskId = task.Id;
            }
        }

        public void HandleUpdateTask(UpdateTaskCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var project = uow.ProjectRepository.Get(command.ProjectId);
                if (project != null && project.User.Id != command.UserId)
                {
                    throw new DomainException("User does not own the project");
                }

                var dbtask = uow.TaskRepository.Get(command.Id);
                if (dbtask.User.Id != command.UserId)
                {
                    throw new DomainException("You cannot update task for another user");
                }

                dbtask.DueDate = DateTime.MaxValue;
                dbtask.IsDone = command.IsDone;
                dbtask.Text = command.Text.Trim();
                dbtask.Project = project;
                uow.Complete();
            }
        }

        public void HandleRemoveTask(RemoveTaskCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var dbtask = uow.TaskRepository.Get(command.TaskId);
                if (dbtask.User.Id != command.UserId)
                {
                    throw new DomainException("You cannot remove task for another user");
                }

                uow.TaskRepository.Remove(dbtask);
                uow.Complete();
            }
        }
        
        public void HandleCheckTask(CheckTaskCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var dbtask = uow.TaskRepository.Get(command.TaskId);
                if (dbtask.User.Id != command.UserId)
                {
                    throw new DomainException("You cannot check/uncheck task for another user");
                }

                dbtask.IsDone = command.IsDone;
                uow.Complete();
            }
        }
    }
}
