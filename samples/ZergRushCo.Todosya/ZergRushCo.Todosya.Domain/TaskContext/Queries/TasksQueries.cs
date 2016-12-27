using System.Collections.Generic;
using System.Linq;
using Saritasa.Tools;
using Saritasa.Tools.Messages.Abstractions;
using ZergRushCo.Todosya.Domain.TaskContext.Dtos;
using ZergRushCo.Todosya.Domain.TaskContext.Entities;

namespace ZergRushCo.Todosya.Domain.TaskContext.Queries
{
    /// <summary>
    /// Tasks queries.
    /// </summary>
    [QueryHandlers]
    public class TasksQueries
    {
        readonly IAppUnitOfWork uow;

        /// <summary>
        /// .ctor for query pipeline.
        /// </summary>
        private TasksQueries()
        {
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="uow">Application unit of work.</param>
        public TasksQueries(IAppUnitOfWork uow)
        {
            this.uow = uow;
        }

        /// <summary>
        /// Get task by id. Returns DTO.
        /// </summary>
        /// <param name="taskId">Task id.</param>
        /// <returns>Task DTO.</returns>
        public TaskDto GetByIdDto(int taskId)
        {
            return new TaskDto(uow.TaskRepository.Get(taskId));
        }

        /// <summary>
        /// Returns user's tasks DTO.
        /// </summary>
        /// <param name="userId">User id the tasks related to.</param>
        /// <returns>Tasks enumerable.</returns>
        public IEnumerable<TaskDto> GetByUserDto(string userId)
        {
            return uow.TaskRepository.Where(t => t.User.Id == userId).ToList().Select(t => new TaskDto(t));
        }
    }
}
