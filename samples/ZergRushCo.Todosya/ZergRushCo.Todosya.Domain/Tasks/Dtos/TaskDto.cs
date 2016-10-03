using System;
using ZergRushCo.Todosya.Domain.Tasks.Entities;

namespace ZergRushCo.Todosya.Domain.Tasks.Dtos
{
    public class TaskDto
    {
        public TaskDto()
        {
        }

        public TaskDto(Task task)
        {
            Id = task.Id;
            UserId = task.User.Id;
            ProjectId = task.Project.Id;
            Text = task.Text;
            IsDone = task.IsDone;
            DueDate = task.DueDate;
        }

        public int Id { get; set; }

        public int UserId { get; set; }

        public int ProjectId { get; set; }

        public string Text { get; set; }

        public bool IsDone { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
