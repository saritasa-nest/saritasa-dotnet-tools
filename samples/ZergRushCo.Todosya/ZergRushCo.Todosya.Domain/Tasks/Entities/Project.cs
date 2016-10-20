using System;
using System.ComponentModel.DataAnnotations;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Tasks.Entities
{
    /// <summary>
    /// Project is a way for tasks grouping.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Project id. Primary key.
        /// </summary>
        [Key]
        public int Id { get; private set; }

        /// <summary>
        /// The user project is related to.
        /// </summary>
        public virtual User User { get; set; }

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
        public string Color { get; set; }

        /// <summary>
        /// When the project has been created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last time the project has been updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
