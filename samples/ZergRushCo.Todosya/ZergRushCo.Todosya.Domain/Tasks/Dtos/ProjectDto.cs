namespace ZergRushCo.Todosya.Domain.Tasks.Dtos
{
    /// <summary>
    /// Simple project dto.
    /// </summary>
    public class ProjectDto
    {
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
