using System.Collections.Generic;
using System.Linq;
using Saritasa.Tools;
using ZergRushCo.Todosya.Domain.Tasks.Dtos;
using ZergRushCo.Todosya.Domain.Tasks.Entities;

namespace ZergRushCo.Todosya.Domain.Tasks.Queries
{
    public class TasksQueries
    {
        readonly IAppUnitOfWork uow;

        private TasksQueries()
        {
        }

        public TasksQueries(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        public TaskDto GetByIdDto(int taskId)
        {
            return new TaskDto(uow.TaskRepository.Get(taskId));
        }

        public IEnumerable<TaskDto> GetByUserDto(int userId)
        {
            return uow.TaskRepository.Where(t => t.User.Id == userId).ToList().Select(t => new TaskDto(t));
        }
    }
}
