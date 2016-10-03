using System;
using Saritasa.Tools.Commands;
using Saritasa.Tools.Exceptions;
using ZergRushCo.Todosya.Domain.Tasks.Commands;

namespace ZergRushCo.Todosya.Domain.Tasks.Handlers
{
    [CommandHandlers]
    public class ProjectHandlers
    {
        public void HandleCreateProject(CreateProjectCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var project = new Entities.Project()
                {
                    Name = command.Name,
                    Color = command.Color,
                    User = uow.UserRepository.Get(command.CreatedByUserId),
                };
                uow.ProjectRepository.Add(project);
                uow.Complete();
                command.ProjectId = project.Id;
            }
        }

        public void HandleUpdateProject(UpdateProjectCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var project = uow.ProjectRepository.Get(command.ProjectId);

                if (project.User.Id != command.UpdatedByUserId)
                {
                    throw new DomainException("Only user who created project can update it.");
                }

                project.Name = command.Name;
                project.Color = command.Color;
                project.UpdatedAt = DateTime.Now;
                uow.Complete();
            }
        }

        public void HandleRemoveProject(RemoveProjectCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var project = uow.ProjectRepository.Get(command.ProjectId);

                if (project.User.Id != command.UpdatedByUserId)
                {
                    throw new DomainException("Only user who created project can remove it.");
                }

                uow.ProjectRepository.Remove(project);
                uow.Complete();
            }
        }
    }
}
