using System;
using System.ComponentModel.DataAnnotations;
using Saritasa.Tools.Messages.Abstractions;

namespace ZergRushCo.Todosya.Domain.TaskContext.Commands
{
    /// <summary>
    /// Create task command.
    /// </summary>
    public class CreateTaskCommand
    {
        /// <summary>
        /// User id the task is related to.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Project id the task is related to. Optional.
        /// </summary>
        public int ProjectId { get; set; }

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
        /// Task completion due date.
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Output of task id.
        /// </summary>
        [CommandOut]
        public int TaskId { get; set; }
    }
}
