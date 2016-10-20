using System;
using System.Collections.Generic;
using System.Linq;
using Saritasa.Tools;
using Saritasa.Tools.Queries;
using ZergRushCo.Todosya.Domain.Tasks.Dtos;
using ZergRushCo.Todosya.Domain.Tasks.Entities;

namespace ZergRushCo.Todosya.Domain.Tasks.Queries
{
    /// <summary>
    /// Projects queries.
    /// </summary>
    [QueryHandlers]
    public class ProjectsQueries
    {
        private readonly IAppUnitOfWork uow;

        /// <summary>
        /// .ctor for query pipeline.
        /// </summary>
        private ProjectsQueries()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="uow">Application unit of work.</param>
        public ProjectsQueries(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        /// <summary>
        /// Get project by id.
        /// </summary>
        /// <param name="projectId">Project id.</param>
        /// <returns>Project.</returns>
        public Project GetById(int projectId)
        {
            return uow.ProjectRepository.Get(projectId);
        }

        /// <summary>
        /// Get user's projects.
        /// </summary>
        /// <param name="userId">User id the projects related to.</param>
        /// <param name="page">Page number.</param>
        /// <param name="pageSize">Page size, default is 10.</param>
        /// <returns>Projects paged enumerable.</returns>
        public PagedEnumerable<Project> GetByUser(int userId, int page, int pageSize = 10)
        {
            return new PagedEnumerable<Project>(uow.ProjectRepository.Find(p => p.User.Id == userId), page, pageSize);
        }

        /// <summary>
        /// Get user's projects. Returns project DTO.
        /// </summary>
        /// <param name="userId">User id the projects related to.</param>
        /// <returns>Projects enumerable.</returns>
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
