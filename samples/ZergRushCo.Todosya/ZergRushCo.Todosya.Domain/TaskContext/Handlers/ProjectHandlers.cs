using System;
using System.Linq;
using Saritasa.Tools.Messages.Abstractions;
using Saritasa.Tools.Domain.Exceptions;
using Saritasa.Tools.Messages.Abstractions.Commands;
using ZergRushCo.Todosya.Domain.TaskContext.Commands;

namespace ZergRushCo.Todosya.Domain.TaskContext.Handlers
{
    /// <summary>
    /// Project related handlers.
    /// </summary>
    [CommandHandlers]
    public class ProjectHandlers
    {
        /// <summary>
        /// Create project handler.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="uowFactory">Application unit of work factory.</param>
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
                uow.SaveChanges();
                command.ProjectId = project.Id;
            }
        }

        /// <summary>
        /// Update project handler.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="uowFactory">Application unit of work factory.</param>
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
                uow.SaveChanges();
            }
        }

        /// <summary>
        /// Remove project handler.
        /// </summary>
        /// <param name="command">Command.</param>
        /// <param name="uowFactory">Application unit of work factory.</param>
        public void HandleRemoveProject(RemoveProjectCommand command, IAppUnitOfWorkFactory uowFactory)
        {
            using (var uow = uowFactory.Create())
            {
                var project = uow.ProjectRepository.Get(command.ProjectId);

                if (project.User.Id != command.UpdatedByUserId)
                {
                    throw new DomainException("Only user who created project can remove it.");
                }

                uow.TaskRepository.RemoveRange(uow.TaskRepository.Where(t => t.Project.Id == project.Id));
                uow.ProjectRepository.Remove(project);
                uow.SaveChanges();
            }
        }
    }
}
