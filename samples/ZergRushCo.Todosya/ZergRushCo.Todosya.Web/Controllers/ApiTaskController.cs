using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Messages.Abstractions;
using ZergRushCo.Todosya.Domain.TaskContext.Queries;
using ZergRushCo.Todosya.Domain.TaskContext.Commands;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Tasks controller for ajax requests.
    /// </summary>
    [Authorize]
    [RoutePrefix("api/tasks")]
    public class ApiTaskController : BaseController
    {
        public ApiTaskController(
            ICommandPipeline commandPipeline,
            IQueryPipeline queryPipeline,
            ILoggerFactory loggerFactory) : base(commandPipeline, queryPipeline, loggerFactory)
        {
        }

        [HttpGet]
        [Route]
        public ActionResult Get()
        {
            var userId = User.Identity.GetUserId();
            var data = QueryPipeline.Query<TasksQueries>().With(q => q.GetByUserDto(userId));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route]
        public ActionResult Post(CreateTaskCommand command)
        {
            var userId = User.Identity.GetUserId();
            command.UserId = userId;
            CommandPipeline.Handle(command);
            var data = QueryPipeline.Query<TasksQueries>().With(q => q.GetByIdDto(command.TaskId));
            return Json(data);
        }

        [HttpPut]
        [Route]
        public ActionResult Put(UpdateTaskCommand command)
        {
            var userId = User.Identity.GetUserId();
            command.UserId = userId;
            CommandPipeline.Handle(command);
            var data = QueryPipeline.Query<TasksQueries>().With(q => q.GetByIdDto(command.Id));
            return Json(data);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public ActionResult Delete(int id)
        {
            var userId = User.Identity.GetUserId();
            var command = new RemoveTaskCommand
            {
                UserId = userId,
                TaskId = id,
            };
            CommandPipeline.Handle(command);
            return Json(true);
        }

        [HttpPost]
        [Route("{id:int}/check")]
        public ActionResult Check(CheckTaskCommand command)
        {
            var userId = User.Identity.GetUserId();
            command.UserId = userId;
            CommandPipeline.Handle(command);
            return Json(true);
        }
    }
}
