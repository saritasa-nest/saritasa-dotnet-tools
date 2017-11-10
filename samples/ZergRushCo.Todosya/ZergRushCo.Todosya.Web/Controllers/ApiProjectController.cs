using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Messages.Abstractions;
using ZergRushCo.Todosya.Domain.TaskContext.Queries;
using Saritasa.Tools.Messages.Abstractions.Queries;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Project controller for ajax requestes.
    /// </summary>
    [Authorize]
    [RoutePrefix("api/projects")]
    public class ApiProjectController : BaseController
    {
        public ApiProjectController(
            IMessagePipelineService pipelinesService,
            ILoggerFactory loggerFactory) :
            base(pipelinesService, loggerFactory)
        {
        }

        [HttpGet]
        [Route]
        public ActionResult Get()
        {
            var userId = User.Identity.GetUserId();
            var data = PipelineService.Query<ProjectsQueries>().With(q => q.GetByUserDto(userId));
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
