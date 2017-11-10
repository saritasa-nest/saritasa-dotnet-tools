using System.ComponentModel.DataAnnotations;

namespace ZergRushCo.Todosya.Domain.TaskContext.Commands
{
    /// <summary>
    /// Create project command.
    /// </summary>
    public class CreateProjectCommand
    {
        /// <summary>
        /// Project id. Out parameter.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Project name.
        /// </summary>
        [Required]
        [MaxLength(125)]
        public string Name { get; set; }

        /// <summary>
        /// Project color.
        /// </summary>
        [MaxLength(10)]
        [ColorValidation]
        public string Color { get; set; } = "#2225AD";

        /// <summary>
        /// User id who creates the project.
        /// </summary>
        public string CreatedByUserId { get; set; }
    }
}
