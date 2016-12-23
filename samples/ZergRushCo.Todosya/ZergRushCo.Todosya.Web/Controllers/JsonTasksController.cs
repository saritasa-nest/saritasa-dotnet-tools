using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Saritasa.Tools.Messages.Abstractions;
using ZergRushCo.Todosya.Domain.Tasks.Queries;
using ZergRushCo.Todosya.Domain.Tasks.Commands;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Tasks controller for ajax requests.
    /// </summary>
    [Authorize]
    [RoutePrefix("json/tasks")]
    public class JsonTasksController : BaseController
    {
        public JsonTasksController(ICommandPipeline commandPipeline, IQueryPipeline queryPipeline) :
            base(commandPipeline, queryPipeline)
        {
        }

        [HttpGet]
        [Route]
        public ActionResult Get()
        {
            var userId = Convert.ToInt32(User.Identity.GetUserId());
            var data = QueryPipeline.Query<TasksQueries>().With(q => q.GetByUserDto(userId));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Route]
        public ActionResult Post(CreateTaskCommand command)
        {
            var userId = Convert.ToInt32(User.Identity.GetUserId());
            command.UserId = userId;
            CommandPipeline.Handle(command);
            var data = QueryPipeline.Query<TasksQueries>().With(q => q.GetByIdDto(command.TaskId));
            return Json(data);
        }

        [HttpPut]
        [Route]
        public ActionResult Put(UpdateTaskCommand command)
        {
            var userId = Convert.ToInt32(User.Identity.GetUserId());
            command.UserId = userId;
            CommandPipeline.Handle(command);
            var data = QueryPipeline.Query<TasksQueries>().With(q => q.GetByIdDto(command.Id));
            return Json(data);
        }

        [HttpDelete]
        [Route("{id:int}")]
        public ActionResult Delete(int id)
        {
            var userId = Convert.ToInt32(User.Identity.GetUserId());
            var command = new RemoveTaskCommand()
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
            var userId = Convert.ToInt32(User.Identity.GetUserId());
            command.UserId = userId;
            CommandPipeline.Handle(command);
            return Json(true);
        }
    }
}
