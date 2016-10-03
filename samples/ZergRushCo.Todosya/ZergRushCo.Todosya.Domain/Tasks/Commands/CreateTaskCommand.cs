using Saritasa.Tools.Commands;
using System;
using System.ComponentModel.DataAnnotations;

namespace ZergRushCo.Todosya.Domain.Tasks.Commands
{
    public class CreateTaskCommand
    {
        public int UserId { get; set; }

        public int ProjectId { get; set; }

        [MaxLength(255)]
        public string Text { get; set; }

        public bool IsDone { get; set; }

        public DateTime? DueDate { get; set; }

        [CommandOut]
        public int TaskId { get; set; }
    }
}
