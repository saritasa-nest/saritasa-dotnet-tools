using System.ComponentModel.DataAnnotations;

namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    /// <summary>
    /// Update project command.
    /// </summary>
    public class UpdateProjectCommand
    {
        /// <summary>
        /// Project id to update.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Project name to update.
        /// </summary>
        [Required]
        [MaxLength(125)]
        public string Name { get; set; }

        /// <summary>
        /// Color to update.
        /// </summary>
        [MaxLength(10)]
        [ColorValidation]
        public string Color { get; set; }

        /// <summary>
        /// User who updates the project.
        /// </summary>
        public int UpdatedByUserId { get; set; }
    }
}
