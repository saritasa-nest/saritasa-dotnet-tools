using System;
using System.ComponentModel.DataAnnotations;
using ZergRushCo.Todosya.Domain.Users.Entities;

namespace ZergRushCo.Todosya.Domain.Tasks.Entities
{
    /// <summary>
    /// Task. This is to-do item for user.
    /// </summary>
    public class Task
    {
        [Key]
        public int Id { get; private set; }

        public virtual User User { get; set; }

        public virtual Project Project { get; set; }

        [MaxLength(255)]
        public string Text { get; set; }

        public bool IsDone { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
