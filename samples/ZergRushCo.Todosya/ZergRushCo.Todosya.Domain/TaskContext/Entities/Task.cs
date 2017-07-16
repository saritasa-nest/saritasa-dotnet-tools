using System;
using System.ComponentModel.DataAnnotations;
using ZergRushCo.Todosya.Domain.UserContext.Entities;

namespace ZergRushCo.Todosya.Domain.TaskContext.Entities
{
    /// <summary>
    /// Task. This is to-do item for user.
    /// </summary>
    public class Task
    {
        /// <summary>
        /// Task id. Primary key.
        /// </summary>
        [Key]
        public int Id { get; private set; }

        /// <summary>
        /// The user task belongs to.
        /// </summary>
        [Required]
        public virtual User User { get; set; }

        /// <summary>
        /// The project task related to. Project must be created by the same user.
        /// </summary>
        public virtual Project Project { get; set; }

        [Required]
        public int? ProjectId { get; set; }

        /// <summary>
        /// Task text.
        /// </summary>
        [MaxLength(255)]
        public string Text { get; set; }

        /// <summary>
        /// Is task done?
        /// </summary>
        public bool IsDone { get; set; }

        /// <summary>
        /// Due date to make task done.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// When task has been created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last time the task has been updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
