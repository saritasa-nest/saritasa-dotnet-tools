using System;
using System.Collections.Generic;
using System.Linq;
using Saritasa.Tools;
using ZergRushCo.Todosya.Domain.Tasks.Dtos;
using ZergRushCo.Todosya.Domain.Tasks.Entities;

namespace ZergRushCo.Todosya.Domain.Tasks.Queries
{
    public class ProjectsQueries
    {
        readonly IAppUnitOfWork uow;

        private ProjectsQueries()
        {
        }

        public ProjectsQueries(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        public Project GetById(int projectId)
        {
            return uow.ProjectRepository.Get(projectId);
        }

        public PagedEnumerable<Project> GetByUser(int userId, int page, int pageSize)
        {
            return new PagedEnumerable<Project>(uow.ProjectRepository.Find(p => p.User.Id == userId), page, pageSize);
        }

        public IEnumerable<ProjectDto> GetByUserDto(int userId)
        {
            return uow.ProjectRepository.Find(p => p.User.Id == userId).Select(p => new ProjectDto()
            {
                Id = p.Id,
                Color = p.Color,
                Name = p.Name,
            });
        }
    }
}
