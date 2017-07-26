using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Extensions.Logging;
using Saritasa.Tools.Messages.Abstractions;
using ZergRushCo.Todosya.Domain.TaskContext.Queries;
using ZergRushCo.Todosya.Domain.TaskContext.Commands;

namespace ZergRushCo.Todosya.Web.Controllers
{
    /// <summary>
    /// Controller contains actions for projects handling.
    /// </summary>
    public class ProjectController : BaseController
    {
        public ProjectController(
            IMessagePipelineService pipelinesService,
            ILoggerFactory loggerFactory) :
            base(pipelinesService, loggerFactory)
        {
        }

        public ActionResult Index(int page = 1)
        {
            var userId = User.Identity.GetUserId();
            return View(PipelineService.Query<ProjectsQueries>().With(q => q.GetByUser(userId, page, 10)));
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new CreateProjectCommand());
        }

        [HttpPost]
        public ActionResult Create(CreateProjectCommand command)
        {
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            command.CreatedByUserId = User.Identity.GetUserId();
            PipelineService.HandleCommand(command);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var project = PipelineService.Query<ProjectsQueries>().With(q => q.GetById(id));
            if (project == null)
            {
                return HttpNotFound();
            }

            return View(new UpdateProjectCommand()
            {
                ProjectId = project.Id,
                Color = project.Color,
                Name = project.Name,
            });
        }

        [HttpPost]
        public ActionResult Edit(int id, UpdateProjectCommand command)
        {
            command.ProjectId = id;
            command.UpdatedByUserId = User.Identity.GetUserId();
            if (!ModelState.IsValid)
            {
                return View(command);
            }

            PipelineService.HandleCommand(command);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Remove(int id)
        {
            var project = PipelineService.Query<ProjectsQueries>().With(q => q.GetById(id));
            if (project == null)
            {
                return HttpNotFound();
            }

            return View(project);
        }

        [HttpPost]
        [ActionName("Remove")]
        public ActionResult RemovePost(int id)
        {
            PipelineService.HandleCommand(new RemoveProjectCommand()
            {
                ProjectId = id,
                UpdatedByUserId = User.Identity.GetUserId(),
            });
            return RedirectToAction("Index");
        }
    }
}
