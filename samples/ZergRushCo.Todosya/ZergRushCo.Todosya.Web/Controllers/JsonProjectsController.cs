using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Saritasa.Tools.Commands;
using Saritasa.Tools.Queries;
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
            var data = QueryPipeline.Execute(QueryPipeline.GetQuery<ProjectsQueries>().GetByUserDto, userId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
