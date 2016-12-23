using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Saritasa.Tools.Messages.Abstractions;
using ZergRushCo.Todosya.Domain.Tasks.Queries;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Project controller for ajax requestes.
    /// </summary>
    [Authorize]
    [RoutePrefix("json/projects")]
    public class JsonProjectsController : BaseController
    {
        public JsonProjectsController(ICommandPipeline commandPipeline, IQueryPipeline queryPipeline) :
            base(commandPipeline, queryPipeline)
        {
        }

        [HttpGet]
        [Route]
        public ActionResult Get()
        {
            var userId = Convert.ToInt32(User.Identity.GetUserId());
            var data = QueryPipeline.Query<ProjectsQueries>().With(q => q.GetByUserDto(userId));
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
