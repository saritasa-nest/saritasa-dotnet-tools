using ZergRushCo.Todosya.Domain.Tasks.Entities;

namespace ZergRushCo.Todosya.Domain.Tasks.Dtos
{
    /// <summary>
    /// Simple project dto.
    /// </summary>
    public class ProjectDto
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public ProjectDto()
        {
        }

        /// <summary>
        /// .ctor to create DTO from project entity.
        /// </summary>
        /// <param name="project">Project entity.</param>
        public ProjectDto(Project project)
        {
            Id = project.Id;
            Name = project.Name;
            Color = project.Color;
        }

        /// <summary>
        /// Project id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Project name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Project color.
        /// </summary>
        public string Color { get; set; }
    }
}
