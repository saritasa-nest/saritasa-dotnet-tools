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
        [Key]
        public int Id { get; private set; }

        public virtual User User { get; set; }

        [Required]
        [MaxLength(125)]
        public string Name { get; set; }

        [MaxLength(10)]
        public string Color { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
