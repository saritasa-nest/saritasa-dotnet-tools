using System;
using ZergRushCo.Todosya.Domain.TaskContext.Entities;

namespace ZergRushCo.Todosya.Domain.TaskContext.Dtos
{
    /// <summary>
    /// Task simple DTO.
    /// </summary>
    public class TaskDto
    {
        /// <summary>
        /// .ctor
        /// </summary>
        public TaskDto()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="task">Task to build DTO from.</param>
        public TaskDto(Task task)
        {
            Id = task.Id;
            UserId = task.User.Id;
            if (task.Project != null)
            {
                ProjectId = task.Project.Id;
            }
            Text = task.Text;
            IsDone = task.IsDone;
            DueDate = task.DueDate;
        }

        /// <summary>
        /// Task id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User id the task is belong to.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Project id the task is related to.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Task text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Is task done?
        /// </summary>
        public bool IsDone { get; set; }

        /// <summary>
        /// Due date to complete the task.
        /// </summary>
        public DateTime? DueDate { get; set; }
    }
}
